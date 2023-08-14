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
           var resetToken = await _authRepo.CreateResetToken(forgotDTo, Request.Headers["origin"]); // fix origin key bug

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

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto emailDto)
        {
            if (emailDto.Email == null)
                return Unauthorized("No email to confirm!");

            var isTokenSaved = await _authRepo.ConfirmEmail(emailDto, Request.Headers["origin"]);

            if (!isTokenSaved)
                return NotFound("No token for verification!");

            return Ok(new
            {
                message = "Confirmation email sent, check your email!"
            });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDto emailDto)
        {
            if (emailDto.Token == null)
                return Unauthorized("No token to verify email!");

            var isEmailVerified = await _authRepo.VerifyEmail(emailDto.Token);

            if (!isEmailVerified)
                return NotFound("Email wasn't verified!"); ;

            return Ok(new
            {
                message = "Verification successful, your email was verified!"
            });
        }

        [HttpPost("verify-email-test")]
        public async Task<IActionResult> VerifyEmailPostmanTest(VerifyEmailDto emailDto)
        {
            if (emailDto.Token == null)
                return Unauthorized("No token to verify email!");

            var isEmailVerified = await _authRepo.VerifyEmailPostmanTest(emailDto);

            if (!isEmailVerified)
                return NotFound("Email wasn't verified!"); ;

            return Ok(new
            {
                message = "Verification successful, your email was verified!"
            });
        }

        [HttpPost("change-email")]
        public async Task<IActionResult> ChangeEmail(ChangeEmailDto emailDto)
        {
            if (emailDto.NewEmail == emailDto.CurrentEmail)
                return Unauthorized("New email is the same as an old one!");

            if (emailDto.Password != emailDto.ConfirmPassword)
                return Unauthorized("Passwords do not match!");

            var isEmailChanged = await _authRepo.ChangeEmail(emailDto, Request.Headers["origin"]);

            if (!isEmailChanged)
                return NotFound("Email wasn't changed!"); ;

            return Ok(new
            {
                message = "Email change successful, verify your new email!"
            });
        }
    }
}
