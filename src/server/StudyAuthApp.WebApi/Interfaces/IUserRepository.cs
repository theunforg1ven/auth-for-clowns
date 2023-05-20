using StudyAuthApp.WebApi.Helpers;
using StudyAuthApp.WebApi.Models;

namespace StudyAuthApp.WebApi.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserById(int id);

        Task<User> GetUserByEmail(string email);

        Task<User> GetUserByUsername(string username);

        Task<List<User>> GetAllUsers();

        Task<List<User>> GetNewestUsers();

        Task<List<User>> GetAllUsersByRole(Role role);

        Task<bool> UserExistsById(int userId);

        Task<bool> UserExistsByEmail(string email);

        Task<bool> UserExistsByUsername(string username);
    }
}
