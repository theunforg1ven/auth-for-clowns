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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> Forgot(ForgotDto forgotDTo)
        {
           var resetToken = await _authRepo.CreateResetToken(forgotDTo, Request.Headers["origin"]);

           var isTokenSaved = await _authRepo.AddResetToken(resetToken);

           if(!isTokenSaved)
               return NotFound("Reset token was not saved!");

            return Ok(new
            {
                message = "Reset link sent!"
            });
        }
        
        [HttpPost("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken(ValidateResetTokenDto resetTokenDto)
        {
            var user  = await _authRepo.ValidateResetToken(resetTokenDto);

            if (user == null)
                return NotFound("No user with reset tokn to validate!");
            
            return Ok(new
            {
                user,
                validationToken = resetTokenDto,
                message = "Token is valid!"
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> Reset(ResetDto resetDto)
        {
            if (resetDto.Password != resetDto.ConfirmPassword)
                return Unauthorized("Passwords do not match!");

            var isPasswordReset = await _authRepo.ResetPassword(resetDto);

            if (!isPasswordReset)
                return Unauthorized("Error of reseting user password!");

            return Ok(new
            {
                message = "Password reset successful, you can now login!"
            });
        }
    }
}
