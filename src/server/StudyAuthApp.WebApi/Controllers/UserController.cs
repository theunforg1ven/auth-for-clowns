using Microsoft.AspNetCore.Mvc;
using StudyAuthApp.WebApi.Interfaces;

namespace StudyAuthApp.WebApi.Controllers
{
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
    }
}
