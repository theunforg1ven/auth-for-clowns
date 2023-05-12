namespace StudyAuthApp.WebApi.DTOs
{
    public class ResetDto
    {
        public string Token { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
