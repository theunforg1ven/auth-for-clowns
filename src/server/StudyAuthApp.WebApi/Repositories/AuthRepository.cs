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

        #region Register & Login

        public async Task<User> Register(User user, string password, string origin)
        {
            if (await _context.Users.AnyAsync(x => x.Email == user.Email))
            {
                await SendAlreadyRegisteredEmail(user.Email, origin);
                return null;
            }

            var hashedPassword = CreatePasswordHash(password);

            user.Password = hashedPassword;
            user.Role = Role.User;

            user.EmailVerificationToken = GenerateVerificationToken();
            user.EmailTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            await SendVerificationEmail(user, origin);

            return user;
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

        #endregion

        #region Refresh tokens

        public async Task<bool> AddUserRefreshToken(UserToken token)
        {
            if (await _context.UserTokens.Where(ut => ut.UserId == token.UserId).AnyAsync())
            {
                var tokensToRemove = await _context.UserTokens
                    .Where(ut => ut.UserId == token.UserId)
                    .ToListAsync();
                _context.UserTokens.RemoveRange(tokensToRemove);
            }

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

        #endregion

        #region Forgot & Reset password

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
                ResetTokenExpires = DateTime.UtcNow.AddMinutes(30),
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

        #endregion

        #region Verify & Change email

        public async Task<bool> ConfirmEmail(ConfirmEmailDto emailDto, string origin)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailDto.Email)
                ?? throw new AppException("No user found to confirm email!");

            if (user.IsEmailVerified)
                return false;

            user.EmailVerificationToken = GenerateVerificationToken();
            user.EmailTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);

            _context.Users.Update(user);
            var savedChanges = await _context.SaveChangesAsync();

            await SendVerificationEmail(user, origin);

            return savedChanges > 0;
        }

        public async Task<bool> VerifyEmailPostmanTest(VerifyEmailDto emailDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == emailDto.Token)
                ?? throw new AppException("No user for verification!");

            if (user.IsEmailVerified || user.EmailTokenExpiresAt < DateTime.UtcNow)
                return false;

            user.EmailVerifiedAt = DateTime.UtcNow;
            user.IsEmailVerified = true;
            user.EmailTokenExpiresAt = default;
            user.EmailVerificationToken = null;

            _context.Users.Update(user);
            var savedChanges = await _context.SaveChangesAsync();

            return savedChanges > 0;
        }

        public async Task<bool> VerifyEmail(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token)
                ?? throw new AppException("No user for verification!");

            if (user.IsEmailVerified || user.EmailTokenExpiresAt < DateTime.UtcNow)
                return false;

            user.EmailVerifiedAt = DateTime.UtcNow;
            user.IsEmailVerified = true;
            user.EmailTokenExpiresAt = default;
            user.EmailVerificationToken = null;

            _context.Users.Update(user);
            var savedChanges = await _context.SaveChangesAsync();

            return savedChanges > 0;
        }

        public async Task<bool> ChangeEmailRequest(ChangeEmailRequestDto emailDto, string origin)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailDto.Email)
                ?? throw new AppException("No user found to change email!");

            user.EmailChangeToken = GenerateVerificationToken();
            user.EmailChangeTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);

            _context.Users.Update(user);
            var savedChanges = await _context.SaveChangesAsync();

            await SendChangeEmail(user, origin);

            return savedChanges > 0;
        }

        public async Task<bool> ChangeEmail(ChangeEmailDto emailDto, string origin)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailDto.CurrentEmail)
                ?? throw new AppException("No user for verification!");

            if (user.EmailChangeTokenExpiresAt < DateTime.UtcNow)
                return false;

            if (!VerifyPasswordHash(emailDto.Password, user.Password))
                return false;

            user.Email = emailDto.NewEmail;

            user.EmailChangeToken = null;
            user.EmailChangeTokenExpiresAt = default;
            user.EmailChangedAt = DateTime.UtcNow;

            user.EmailVerifiedAt = default;
            user.IsEmailVerified = false;
            user.EmailVerificationToken = GenerateVerificationToken();
            user.EmailTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);

            _context.Users.Update(user);

            await SendVerificationEmail(user, origin);

            var savedChanges = await _context.SaveChangesAsync();

            return savedChanges > 0;
        }

        #endregion

        #region Generate tokens private helpers

        private string GenerateResetToken()
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

            var tokenIsUnique = !_context.ResetTokens.Any(x => x.Token == token);
            if (!tokenIsUnique)
                return GenerateResetToken();

            return token;
        }

        private string GenerateVerificationToken()
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

            var tokenIsUnique = !_context.Users.Any(u => u.EmailVerificationToken == token);
            if (!tokenIsUnique)
                return GenerateVerificationToken();

            return token;
        }

        #endregion

        #region Send emails private helpers

        private async Task SendVerificationEmail(User user, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var verifyUrl = $"{origin}/account/verify-email?token={user.EmailVerificationToken}";
                message = $@"<p>Please click the below link to verify your email address:</p>
                            <p><a href=""{verifyUrl}"">verify your email</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to verify your email address</p>
                            <p><code>{user.EmailVerificationToken}</code></p>";
            }

            await _emailService.Send(
                to: user.Email,
                subject: "Email verification",
                html: $@"<h4>Verify Email</h4>
                        {message}"
            );
        }

        private async Task SendChangeEmail(User user, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var verifyUrl = $"{origin}/profile/change-email?token={user.EmailChangeToken}";
                message = $@"<p>Please click the below link to change your email address:</p>
                            <p><a href=""{verifyUrl}"">change your email</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to change your email address</p>
                            <p><code>{user.EmailChangeToken}</code></p>";
            }

            await _emailService.Send(
                to: user.Email,
                subject: "Email change",
                html: $@"<h4>Change Email</h4>
                        {message}"
            );
        }

        private async Task SendPasswordResetEmail(ResetToken resetToken, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var resetUrl = $"{origin}/account/reset-password?token={resetToken.Token}";
                message = $@"<p>Please click the below link to reset your password! Link is available for 30 minutes</p>
                            <p><a href=""{resetUrl}"">reset password</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to reset your password :)</p>
                            <p><code>{resetToken.Token}</code></p>";
            }

            await _emailService.Send(
                to: resetToken.Email,
                subject: "Password Reset",
                html: $@"<h4>Reset Password Email</h4>
                        {message}"
            );
        }

        private async Task SendAlreadyRegisteredEmail(string email, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
                message = $@"<p>If you don't know your password please visit the <a href=""{origin}/account/forgot-password"">forgot password</a> page.</p>";
            else
                message = "<p>If you forgot your password you can reset it!</p>";

            await _emailService.Send(
                to: email,
                subject: "Register Verification - Your email is already registered!",
                html: $@"<h4>Email Already Registered</h4>
                        <p>Your email <strong>{email}</strong> is already in use.</p>
                        {message}"
            );
        }

        #endregion

        #region Hash passwords private helpers

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

        #endregion
    }
}
