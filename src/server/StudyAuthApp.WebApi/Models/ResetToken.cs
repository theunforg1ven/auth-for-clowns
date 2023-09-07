using System.ComponentModel.DataAnnotations;

namespace StudyAuthApp.WebApi.Models
{
    public class ResetToken
    {
        [Key()]
        public string Email { get; set; }

        public string Token { get; set; }

        public DateTime ResetTokenExpires { get; set; } = DateTime.UtcNow.AddHours(1);

        public int UserId { get; set; }

        public User User { get; set; }
    }
}
