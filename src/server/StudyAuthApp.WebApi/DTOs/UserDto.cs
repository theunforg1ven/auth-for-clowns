using StudyAuthApp.WebApi.Helpers;

namespace StudyAuthApp.WebApi.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public Role Role { get; set; }

        public string Jwt { get; set; }
    }
}
