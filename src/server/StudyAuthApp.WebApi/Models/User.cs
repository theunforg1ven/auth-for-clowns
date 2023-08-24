using StudyAuthApp.WebApi.Helpers;

namespace StudyAuthApp.WebApi.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public Role Role { get; set; } = Role.User;

        public bool IsEmailVerified { get; set; }

        public DateTime EmailVerifiedAt { get; set; }

        public DateTime EmailTokenExpiresAt { get; set; }

        public string EmailVerificationToken { get; set; }

        public DateTime EmailChangedAt { get; set; }

        public DateTime EmailChangeTokenExpiresAt { get; set; }

        public string EmailChangeToken { get; set; }
    }
}
