using StudyAuthApp.WebApi.Models;

namespace StudyAuthApp.WebApi.Interfaces
{
    public interface IAuthRepository
    {
        Task<User> Register(User user, string password);

        Task<User> Login(string username, string password);

        Task<bool> AddUserRefreshToken(UserToken token);

        Task<bool> DeleteUserRefreshToken(string refreshToken);

        Task<bool> IsRefreshTokenAvailable(int userId, string refreshToken);

        Task<bool> AddResetToken(ResetToken resetToken);
    }
}
