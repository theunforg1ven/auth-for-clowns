using Microsoft.EntityFrameworkCore;
using StudyAuthApp.WebApi.Data;
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

        public async Task<User> GetUserById(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;

            return user;
        }

        public async Task<bool> UserExists(string username)
            => await _context.Users.AnyAsync(x => x.Username == username);
    }
}
