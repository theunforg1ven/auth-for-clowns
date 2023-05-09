﻿using System.ComponentModel.DataAnnotations;

namespace StudyAuthApp.WebApi.Models
{
    public class ResetToken
    {
        [Key()]
        public string Email { get; set; }

        public string Token { get; set; }
    }
}
