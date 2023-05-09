using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using StudyAuthApp.WebApi.Data;
using StudyAuthApp.WebApi.Interfaces;
using StudyAuthApp.WebApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace StudyAuthApp.WebApi.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public AuthRepository(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<User> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return null;

           if (!VerifyPasswordHash(password, user.Password))
              return null;

            return user;
        }

        public async Task<bool> AddUserRefreshToken(UserToken token)
        {
            await _context.UserTokens.AddAsync(token);
            var savedChanges = await _context.SaveChangesAsync();

            return savedChanges > 0;
        }

        public async Task<bool> DeleteUserRefreshToken(string refreshToken)
        {
            var userToken = await _context.UserTokens.FirstOrDefaultAsync(ut => ut.Token == refreshToken);
            _context.UserTokens.Remove(userToken);
            var savedChanges = await _context.SaveChangesAsync();

            return savedChanges > 0;
        }

        public async Task<bool> IsRefreshTokenAvailable(int userId, string refreshToken)
        {
            var isAvailable = await _context.UserTokens
                .Where(ut => ut.UserId == userId && ut.Token == refreshToken && ut.ExpiredAt > DateTime.Now)
                .AnyAsync();

            return isAvailable;
        }

        public async Task<User> Register(User user, string password)
        {
            var hashedPassword = CreatePasswordHash(password);

            user.Password = hashedPassword;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> AddResetToken(ResetToken resetToken)
        {
            if (resetToken == null)
                return false;

           await _context.ResetTokens.AddAsync(resetToken);
           var savedChanges =  await _context.SaveChangesAsync();

           return savedChanges > 0;
        }

        private string CreatePasswordHash(string password)
        {
            byte[] passwordHash;

            // HMAC
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_config.GetSection("Secrets:SaltKey").Value));
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));        

            // Convert bytes arrays to string
            var hashedPasswordFirst = Convert.ToBase64String(passwordHash);

            // BCrypt
            var hashedPasswordBcrypt = BCrypt.Net.BCrypt.EnhancedHashPassword(hashedPasswordFirst, HashType.SHA512);

            return hashedPasswordBcrypt;
        }

        private bool VerifyPasswordHash(string password, string userPassword)
        {
            // HMAC
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_config.GetSection("Secrets:SaltKey").Value));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Convert bytes arrays to string
            var hPasswordBCrypt = Convert.ToBase64String(computedHash);

            // BCrypt
            var isVerifiedPassword = BCrypt.Net.BCrypt.EnhancedVerify(hPasswordBCrypt, userPassword, HashType.SHA512);

            return isVerifiedPassword;
        }
    }
}
