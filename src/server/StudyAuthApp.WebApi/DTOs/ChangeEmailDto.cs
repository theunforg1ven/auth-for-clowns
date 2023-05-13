namespace StudyAuthApp.WebApi.DTOs
{
    public class ChangeEmailDto
    {
        public string CurrentEmail { get; set; }

        public string NewEmail { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
