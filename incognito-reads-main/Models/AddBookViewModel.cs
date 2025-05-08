using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace IncognitoReads.Models
{
    public class AddBookViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public IFormFile? CoverImage { get; set; }

        public string CoverImageUrl { get; set; } = "/images/covers/default-cover.png";

        public string Color { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public DateTime DateAdded { get; set; }

        public string UserId { get; set; } = string.Empty;
    }
}
