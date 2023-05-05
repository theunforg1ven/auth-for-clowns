using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyAuthApp.WebApi.DTOs;
using StudyAuthApp.WebApi.Interfaces;
using StudyAuthApp.WebApi.Models;

namespace StudyAuthApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;
        private readonly ITokenService _tokenService;

        public AuthController(IAuthRepository repo, ITokenService tokenService)
        {
            _authRepo = repo;
            _tokenService = tokenService;
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();

            if (authorizationHeader == null || authorizationHeader.Length <= 8)
                return Unauthorized("User is unauthorized!");

            var accessToken = authorizationHeader[7..];

            var id = _tokenService.DecodeToken(accessToken, out bool hasTokenExpired);

            if(id == -1)
                return Unauthorized("No user token found!");

            if(hasTokenExpired)
                return Unauthorized("Token is expired!");

            var user = await _authRepo.GetUserById(id);

            if (user == null)
                return Unauthorized("No user found!");

            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDTo)
        {
            if (registerDTo.Password != registerDTo.ConfirmPassword)
                return Unauthorized("Passwords do not match!");

            if (registerDTo.Email != registerDTo.ConfirmEmail)
                return Unauthorized("Emails do not match!");

            var user = new User()
            {
                FirstName = registerDTo.FirstName,
                LastName = registerDTo.LastName,
                Username = registerDTo.Username,
                Email = registerDTo.Email,
                Password = registerDTo.Password,
            };

            var createdUser = await _authRepo.Register(user, user.Password);

            var registeredUser = new RegisteredUserDto()
            {
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                Username = createdUser.Username,
                Email = createdUser.Email,
            };

            return Ok(registeredUser);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var userFromRepo = await _authRepo.Login(loginDto.Email, loginDto.Password);

            if (userFromRepo == null)
                return Unauthorized("Invalid email or password!");

            var accessToken = _tokenService.CreateAccessToken(userFromRepo.Id);
            var refreshToken = _tokenService.CreateRefreshToken(userFromRepo.Id);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            return Ok(new
            {
                token = accessToken,
            });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"]?.ToString();

            if (refreshToken == null)
                return Unauthorized("User is unauthorized!");

            var id = _tokenService.DecodeToken(refreshToken, out bool hasTokenExpired);

            if (id == -1)
                return Unauthorized("No user token found!");

            if (hasTokenExpired)
                return Unauthorized("Token is expired!");

            var accessToken = _tokenService.CreateAccessToken(id);

            return Ok(new
            {
                token = accessToken,
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"]?.ToString();

            if (refreshToken == null)
                return Ok("User is already logged out!");

            Response.Cookies.Delete("refreshToken");

            return Ok("Logged out without problems :)");
        }
    }
}
