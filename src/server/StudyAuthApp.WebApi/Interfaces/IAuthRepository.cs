using StudyAuthApp.WebApi.DTOs;
using StudyAuthApp.WebApi.Models;

namespace StudyAuthApp.WebApi.Interfaces
{
    public interface IAuthRepository
    {
        Task<User> Register(User user, string password, string origin);

        Task<User> Login(string username, string password);

        Task<bool> AddUserRefreshToken(UserToken token);

        Task<bool> DeleteUserRefreshToken(string refreshToken);

        Task<bool> IsRefreshTokenAvailable(int userId, string refreshToken);

        Task<ResetToken> CreateResetToken(ForgotDto forgotDto, string origin);

        Task<bool> AddResetToken(ResetToken resetToken);

        Task<User> ValidateResetToken(ValidateResetTokenDto resetTokenDto);

        Task<bool> ResetPassword(ResetDto resetDto);

        Task<bool> ConfirmEmail(ConfirmEmailDto emailDto, string origin);

        Task<bool> VerifyEmail(string token);

        Task<bool> VerifyEmailPostmanTest(VerifyEmailDto emailDto);

        Task<bool> ChangeEmail(ChangeEmailDto emailDto, string origin);
    }
}
