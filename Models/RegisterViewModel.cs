using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace IncognitoReads.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // 1) Default profilio nuotraukos URL
        public string ProfileImageUrl { get; set; } = "/images/profile-placeholder.jpg";
    }
}
