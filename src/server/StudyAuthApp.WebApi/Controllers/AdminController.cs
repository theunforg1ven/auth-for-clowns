using Microsoft.AspNetCore.Mvc;
using StudyAuthApp.WebApi.AuthorizeHelpers;
using StudyAuthApp.WebApi.DTOs;
using StudyAuthApp.WebApi.Helpers;
using StudyAuthApp.WebApi.Interfaces;
using StudyAuthApp.WebApi.Models;
using StudyAuthApp.WebApi.Services;

namespace StudyAuthApp.WebApi.Controllers
{
    [ApiController]
    [Authorize(Role.Admin)]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public AdminController(IAdminRepository adminRepository, 
                                IUserRepository userRepository, 
                                ITokenService tokenService)
        {
            _adminRepository = adminRepository;
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        [HttpGet("user-by-id")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _adminRepository.GetUserById(id);

            if (user == null)
                return NotFound("No user found with such identificator!");

            return Ok(user);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminRepository.GetAllUsers();

            if (users == null)
                return NotFound("No users found registered in application!");

            return Ok(users);
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> Create(CreateUserDto createUserDto)
        {
            var isUserCreated = await _adminRepository.Create(createUserDto);

            if (!isUserCreated)
                return BadRequest("Error creating new user!");

            return Ok(new
            {
                message = "User was successfully created!",
                createUserDto
            });
        }

        [HttpPut("update-user")]
        public async Task<IActionResult> Update(int id, UpdateUserDto updateUserDto)
        {
            var currentUser = await GetCurrentUser();

            if (id != currentUser.Id && currentUser.Role != Role.Admin)
                return Unauthorized("You don't have rights to do update!");

            if (currentUser.Role != Role.Admin)
                updateUserDto.Role = (int)currentUser.Role;

            var isUserUpdated = await _adminRepository.Update(id, updateUserDto);

            if (!isUserUpdated)
                return BadRequest("Error updating user!");

            return Ok(new
            {
                message = "User was successfully updated!",
            });
        }

        [HttpDelete("delete-user")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await GetCurrentUser();

            if (id != currentUser.Id && currentUser.Role != Role.Admin)
                return Unauthorized("You don't have rights to do update!");

            await _adminRepository.Delete(id);
            
            return Ok(new 
            { 
                message = "Account deleted successfully" 
            });
        }

        private async Task<User> GetCurrentUser()
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();

            if (authorizationHeader == null || authorizationHeader.Length <= 8)
                throw new AppException($"No authorization header found!");

            var accessToken = authorizationHeader[7..];

            var id = _tokenService.DecodeToken(accessToken, out bool hasTokenExpired);

            if (id == -1)
                return null;

            if (hasTokenExpired)
                throw new AppException($"No current user, token has expired!");

            var user = await _userRepository.GetUserById(id);

            if (user == null)
                throw new AppException($"No user found!");

            return user;
        }
    }
}
