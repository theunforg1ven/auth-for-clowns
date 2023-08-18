using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using StudyAuthApp.WebApi.Data;
using StudyAuthApp.WebApi.DTOs;
using StudyAuthApp.WebApi.Extensions;
using StudyAuthApp.WebApi.Helpers;
using StudyAuthApp.WebApi.Interfaces;
using StudyAuthApp.WebApi.Models;
using System;
using System.Security.Cryptography;
using System.Text;

namespace StudyAuthApp.WebApi.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        public AdminRepository(DataContext context, IConfiguration config)
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

        public async Task<List<User>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();

            if (users == null)
                return null;

            return users;
        }

        public async Task<bool> Create(CreateUserDto createUserDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
                throw new AppException($"Email '{createUserDto.Email}' is already registered");

            if (typeof(Role).IsEnumDefined(createUserDto.Role) == false)
                throw new AppException($"No such role: {createUserDto.Role} in current application");

            var hashedPassword = CreatePasswordHash(createUserDto.Password);

            var user = new User()
            {
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                Role = (Role)createUserDto.Role,
                Password = hashedPassword,
            };

            await _context.Users.AddAsync(user);
            var isChanged = await _context.SaveChangesAsync();

            return isChanged > 0;
        }

        public async Task<bool> Update(int id, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new AppException("No user found to update!");

            if (user.Email != updateUserDto.Email 
                    && await _context.Users.AnyAsync(x => x.Email == updateUserDto.Email))
                throw new AppException($"Email '{updateUserDto.Email}' is already registered");

            if (string.IsNullOrEmpty(updateUserDto.Email))
            {
                user.EmailVerifiedAt = default;
                user.IsEmailVerified = false;
            }
                
            if (!string.IsNullOrEmpty(updateUserDto.Password))
                user.Password = CreatePasswordHash(updateUserDto.Password);

            user.FirstName = updateUserDto.FirstName ?? user.FirstName;
            user.LastName = updateUserDto.LastName ?? user.LastName;
            user.Username = updateUserDto.Username ?? user.Username;
            user.Email = updateUserDto.Email ?? user.Email;
            user.Role = updateUserDto.Role.ParseEnum<Role>();

            _context.Users.Update(user);
            var isChanged = await _context.SaveChangesAsync();

            return isChanged > 0;
        }

        public async Task<bool> Delete(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new AppException("No user found to delete!");

            _context.Users.Remove(user);
            
            var isChanged = await _context.SaveChangesAsync();
            return isChanged > 0;
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
    }
}
