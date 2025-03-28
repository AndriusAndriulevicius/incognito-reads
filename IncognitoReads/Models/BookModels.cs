using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IncognitoReads.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Color { get; set; } = "#FFFFFF";
        public List<BookNote> Notes { get; set; } = new List<BookNote>();
    }

  public class BookNote
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Citations { get; set; } = string.Empty;  // Comma-separated string
    public string Links { get; set; } = string.Empty;      // Comma-separated string
    public DateTime CreatedAt { get; set; }
}

    public class BookViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Status { get; set; } = "To Read";
        public DateTime? DateAdded { get; set; }
    }

   public class LibraryViewModel
{
    public List<BookViewModel> Books { get; set; } = new List<BookViewModel>();
}


   public class BookNoteViewModel
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public List<BookNote> Notes { get; set; } = new List<BookNote>();
    public BookNote NewNote { get; set; } = new BookNote();
}
}
