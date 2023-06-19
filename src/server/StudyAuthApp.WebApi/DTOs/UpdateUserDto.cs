﻿namespace StudyAuthApp.WebApi.DTOs
{
    public class UpdateUserDto
    {  
        public string FirstName { get; set; }
       
        public string LastName { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public int Role { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
