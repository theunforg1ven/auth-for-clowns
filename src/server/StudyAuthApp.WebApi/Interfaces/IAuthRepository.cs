using StudyAuthApp.WebApi.Models;

namespace StudyAuthApp.WebApi.Interfaces
{
    public interface IAuthRepository
    {
        Task<User> Register(User user, string password);

        Task<User> Login(string username, string password);

        Task<User> GetUserById(int id);

        Task<bool> UserExists(string username);
    }
}
