using Microsoft.AspNetCore.Mvc;
using StudyAuthApp.WebApi.DTOs;
using StudyAuthApp.WebApi.Interfaces;
using StudyAuthApp.WebApi.Models;

namespace StudyAuthApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;
        private readonly ITokenService _tokenService;

        public EmailController(IAuthRepository repo, ITokenService tokenService)
        {
            _authRepo = repo;
            _tokenService = tokenService;
        }

        [HttpPost("forgot")]
        public async Task<IActionResult> Forgot(ForgotDto forgotDTo)
        {
            var resetToken = new ResetToken()
            {
                Email = forgotDTo.Email,
                Token = Guid.NewGuid().ToString()  
            };

            var isTokenSaved = _authRepo.AddResetToken(resetToken);

            if(isTokenSaved == null)
                return NotFound("Reset token was not saved!");

            return Ok(new
            {
                message = "Success"
            });
        }
    }
}
