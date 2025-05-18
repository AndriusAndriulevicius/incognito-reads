using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace IncognitoReads.Models
{
    public class ReviewViewModel
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        [Required(ErrorMessage = "Book title is required")]
        public string BookTitle { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;

        public IFormFile? CoverImage { get; set; }

        [Required(ErrorMessage = "Please enter your review")]
        public string ReviewText { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Please provide a rating between 1 and 5")]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; } = string.Empty;
    }
}
