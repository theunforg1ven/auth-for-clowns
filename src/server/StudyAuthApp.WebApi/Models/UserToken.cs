namespace StudyAuthApp.WebApi.Models
{
    public class UserToken
    {
        public int Id { get; set; }

        public string Token { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime ExpiredAt { get; set; }

        public int UserId { get; set; }
    }
}
