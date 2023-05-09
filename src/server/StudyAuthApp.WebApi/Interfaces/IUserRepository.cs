using StudyAuthApp.WebApi.Models;

namespace StudyAuthApp.WebApi.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserById(int id);

        Task<bool> UserExists(string username);
    }
}
