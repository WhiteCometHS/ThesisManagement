﻿using System.ComponentModel.DataAnnotations;

namespace DiplomaManagement.Models
{
    public class LoginModel
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string? LoginInValid { get; set; }
    }
}
