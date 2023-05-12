using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using StudyAuthApp.WebApi.Data;
using StudyAuthApp.WebApi.DTOs;
using StudyAuthApp.WebApi.Helpers;
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
        private readonly IEmailService _emailService;

        public AuthRepository(DataContext context, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
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
            var tokensForRemoval = await _context.UserTokens.Where(ut => ut.UserId == userToken.UserId).ToListAsync();

            _context.UserTokens.RemoveRange(tokensForRemoval);
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

        public async Task<User> Register(User user, string password, string origin)
        {
            if (await _context.Users.AnyAsync(x => x.Email == user.Email))
            {
                await SendAlreadyRegisteredEmail(user.Email, origin);
                return null;
            }

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

            var existingToken = await _context.ResetTokens
                .FirstOrDefaultAsync(r => r.Email == resetToken.Email);

            if(existingToken != null)
                _context.ResetTokens.Remove(existingToken);

           await _context.ResetTokens.AddAsync(resetToken);
           var savedChanges =  await _context.SaveChangesAsync();

           return savedChanges > 0;
        }

        public async Task<ResetToken> CreateResetToken(ForgotDto forgotDto, string origin)
        {
            var resetToken = new ResetToken()
            {
                Email = forgotDto.Email,
                Token = GenerateResetToken(),
                ResetTokenExpires = DateTime.UtcNow.AddDays(1),
            };

            // Send email with created Reset Token
            await SendPasswordResetEmail(resetToken, origin);

            return resetToken;
        }

        public async Task<User> ValidateResetToken(ValidateResetTokenDto resetTokenDto)
        {
            var resetToken = await _context.ResetTokens
                .FirstOrDefaultAsync(r => r.Token == resetTokenDto.Token && r.ResetTokenExpires > DateTime.UtcNow) 
                ?? throw new AppException("Invalid token!");
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetToken.Email)
                ?? throw new AppException("No user found to validate!");
           
            return user;
        }

        public async Task<bool> ResetPassword(ResetDto resetDto)
        {
            var resetToken = await _context.ResetTokens
                .FirstOrDefaultAsync(r => r.Token == resetDto.Token && r.ResetTokenExpires > DateTime.UtcNow)
                ?? throw new AppException("Invalid token!");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetToken.Email)
                ?? throw new AppException("No user found to validate!");

            user.Password = CreatePasswordHash(resetDto.Password);

            _context.Users.Update(user);
            _context.ResetTokens.Remove(resetToken);
            var savedChanges = await _context.SaveChangesAsync();

            return savedChanges > 0;
        }

        private string GenerateResetToken()
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

            var tokenIsUnique = !_context.ResetTokens.Any(x => x.Token == token);
            if (!tokenIsUnique)
                return GenerateResetToken();

            return token;
        }

        //private async Task SendVerificationEmail(User user, string origin)
        //{
        //    string message;
        //    if (!string.IsNullOrEmpty(origin))
        //    {
        //        // origin exists if request sent from browser single page app (e.g. Angular or React)
        //        // so send link to verify via single page app
        //        var verifyUrl = $"{origin}/api/email/verify-email?token={user.VerificationToken}";
        //        message = $@"<p>Please click the below link to verify your email address:</p>
        //                    <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
        //    }
        //    else
        //    {
        //        // origin missing if request sent directly to api (e.g. from Postman)
        //        // so send instructions to verify directly with api
        //        message = $@"<p>Please use the below token to verify your email address</p>
        //                    <p><code>{user.VerificationToken}</code></p>";
        //    }

        //    await _emailService.Send(
        //        to: user.Email,
        //        subject: "Sign-up Verification API - Verify Email",
        //        html: $@"<h4>Verify Email</h4>
        //                {message}"
        //    );
        //}

        private async Task SendPasswordResetEmail(ResetToken resetToken, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var resetUrl = $"{origin}/api/email/reset-password?token={resetToken.Token}";
                message = $@"<p>Please click the below link to reset your password, the link will be valid for 1 day:</p>
                            <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to reset your password :)</p>
                            <p><code>{resetToken.Token}</code></p>";
            }

            await _emailService.Send(
                to: resetToken.Email,
                subject: "Sign-up Verification API - Reset Password",
                html: $@"<h4>Reset Password Email</h4>
                        {message}"
            );
        }

        private async Task SendAlreadyRegisteredEmail(string email, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
                message = $@"<p>If you don't know your password please visit the <a href=""{origin}/api/email/forgot-password"">forgot password</a> page.</p>";
            else
                message = "<p>If you forgot your password you can reset it!</p>";

            await _emailService.Send(
                to: email,
                subject: "Sign-up Verification API - Email Already Registered",
                html: $@"<h4>Email Already Registered</h4>
                        <p>Your email <strong>{email}</strong> is already registered.</p>
                        {message}"
            );
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
