using StudyAuthApp.WebApi.DTOs;
using StudyAuthApp.WebApi.Models;

namespace StudyAuthApp.WebApi.Interfaces
{
    public interface IAdminRepository
    {
        Task<User> GetUserById(int id);

        Task<List<User>> GetAllUsers();

        Task<bool> Create(CreateUserDto createUserDto);

        Task<bool> Update(int id, UpdateUserDto updateUserDto);

        Task<bool> Delete(int id);
    }
}
