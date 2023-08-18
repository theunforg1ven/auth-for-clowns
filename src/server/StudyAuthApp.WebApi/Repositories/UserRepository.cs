using Microsoft.EntityFrameworkCore;
using StudyAuthApp.WebApi.Data;
using StudyAuthApp.WebApi.DTOs;
using StudyAuthApp.WebApi.Helpers;
using StudyAuthApp.WebApi.Interfaces;
using StudyAuthApp.WebApi.Models;

namespace StudyAuthApp.WebApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public UserRepository(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        #region Get User methods

        public async Task<User> GetUserById(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;

            return user;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            return user;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return null;

            return user;
        }

        public async Task<List<User>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();

            if (users == null)
                return null;

            return users;
        }


        public async Task<List<User>> GetNewestUsers()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.Id)
                .Take(3)
                .ToListAsync();

            if (users == null)
                return null;

            return users;
        }

        public async Task<List<User>> GetAllUsersByRole(Role role)
        {
            var users = await _context.Users
                .Where(u => u.Role == role)
                .ToListAsync();

            if (users == null)
                return null;

            return users;
        }

        #endregion

        #region Is User exist

        public async Task<bool> UserExistsById(int userId)
            => await _context.Users.AnyAsync(x => x.Id == userId);

        public async Task<bool> UserExistsByEmail(string email)
            => await _context.Users.AnyAsync(x => x.Email == email);

        public async Task<bool> UserExistsByUsername(string username)
            => await _context.Users.AnyAsync(x => x.Username == username);

        #endregion

        public async Task<bool> UpdateInformation(int id, UpdateUserInfoDto updateUserInfoDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new AppException("No user found to update!");

            user.FirstName = updateUserInfoDto.FirstName ?? user.FirstName;
            user.LastName = updateUserInfoDto.LastName ?? user.LastName;
            user.Username = updateUserInfoDto.Username ?? user.Username;
            user.Role = (Role)updateUserInfoDto.Role;

            _context.Users.Update(user);
            var isChanged = await _context.SaveChangesAsync();

            return isChanged > 0;
        }
    }
}
