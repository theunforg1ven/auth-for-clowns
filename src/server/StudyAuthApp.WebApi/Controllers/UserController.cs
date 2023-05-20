using Microsoft.AspNetCore.Mvc;
using StudyAuthApp.WebApi.AuthorizeHelpers;
using StudyAuthApp.WebApi.Helpers;
using StudyAuthApp.WebApi.Interfaces;

namespace StudyAuthApp.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly ITokenService _tokenService;

        public UserController(IUserRepository repo, ITokenService tokenService)
        {
            _userRepo = repo;
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

            if (id == -1)
                return Unauthorized("No user token found!");

            if (hasTokenExpired)
                return Unauthorized("Token is expired!");

            var user = await _userRepo.GetUserById(id);

            if (user == null)
                return Unauthorized("No user found!");

            return Ok(user);
        }

        [HttpGet("user-by-id")]
        public async Task<IActionResult> GetUserById( int id)
        {
            var user = await _userRepo.GetUserById(id);

            if (user == null)
                return NotFound("No user found with such identificator!");

            return Ok(user);
        }

        [HttpGet("user-by-email")]
        public async Task<IActionResult> GetUserByEmail( string email)
        {
            var user = await _userRepo.GetUserByEmail(email);

            if (user == null)
                return NotFound("No user found with such email!");

            return Ok(user);
        }

        [HttpGet("user-by-username")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var user = await _userRepo.GetUserByUsername(username);

            if (user == null)
                return NotFound("No user found with such username!");

            return Ok(user);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepo.GetAllUsers();

            if (users == null)
                return NotFound("No users found registered in application!");

            return Ok(users);
        }

        [HttpGet("users-new")]
        public async Task<IActionResult> GetNewUsers()
        {
            var users = await _userRepo.GetNewestUsers();

            if (users == null)
                return NotFound("No users found registered in application!");

            return Ok(users);
        }

        [HttpGet("users-by-role")]
        public async Task<IActionResult> GetUsersByRole(int role)
        {
            if (typeof(Role).IsEnumDefined(role) == false) 
                return NotFound("No such role in current application");

            var users = await _userRepo.GetAllUsersByRole((Role)role);

            if (users == null)
                return NotFound("No users found registered in application!");

            return Ok(users);
        }

        [HttpGet("user-id-exist")]
        public async Task<IActionResult> IsUserExistById(int id)
        {
            var user = await _userRepo.UserExistsById(id);

            if (user == false)
                return NotFound("Such user does not exist!");

            return Ok("User exist");
        }

        [HttpGet("user-email-exist")]
        public async Task<IActionResult> IsUserExistByEmail(string email)
        {
            var user = await _userRepo.UserExistsByEmail(email);

            if (user == false)
                return NotFound("Such user does not exist!");

            return Ok("User exist");
        }

        [HttpGet("user-username-exist")]
        public async Task<IActionResult> IsUserExistByUsername(string username)
        {
            var user = await _userRepo.UserExistsByUsername(username);

            if (user == false)
                return NotFound("Such user does not exist!");

            return Ok("User exist");
        }
    }
}
