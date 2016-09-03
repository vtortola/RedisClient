using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleQA.Models
{
    public sealed class LoginModel
    {
        [Required]
        public String Username { get; set; }
        public String Password { get; set; }
        public String ReturnUrl { get; set; }
    }
}