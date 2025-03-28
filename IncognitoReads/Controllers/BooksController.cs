using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IncognitoReads.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IncognitoReads.Controllers
{
    public class BooksController : Controller
    {
        private readonly ILogger<BooksController> _logger;
        
        // Pakeisim, kai duombazę turėsim
        private static List<Book> _books = new List<Book>
        {
            new Book { Id = 1, Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Color = "#FF0000" },
            new Book { Id = 2, Title = "To Kill a Mockingbird", Author = "Harper Lee", Color = "#00FF00" },
            new Book { Id = 3, Title = "Pride and Prejudice", Author = "Jane Austen", Color = "#FF00FF" }
        };
        
        private static List<BookNote> _notes = new List<BookNote>();
        
        public BooksController(ILogger<BooksController> logger)
        {
            _logger = logger;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new LibraryViewModel
            {
                Books = _books.Select(b => new BookViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = "",  
                    Status = "To Read", 
                    DateAdded = null 
                }).ToList()
            };

            return View(viewModel);
        }
        
        [HttpGet]
        public IActionResult Details(int id)
        {
            var book = _books.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            
            book.Notes = _notes.Where(n => n.BookId == id).ToList();
            return View(book);
        }
        
        [HttpGet]
        public IActionResult Notes(int bookId)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book == null)
            {
                return NotFound();
            }
            
            var viewModel = new BookNoteViewModel
            {
                BookId = bookId,
                BookTitle = book.Title,
                Notes = _notes.Where(n => n.BookId == bookId).ToList(),
                NewNote = new BookNote { BookId = bookId }
            };
            
            return View(viewModel);
        }
        
        [HttpPost]
        public IActionResult SaveNote(BookNote note)
        {
            if (!ModelState.IsValid)
            {
                var book = _books.FirstOrDefault(b => b.Id == note.BookId);
                var viewModel = new BookNoteViewModel
                {
                    BookId = note.BookId,
                    BookTitle = book?.Title ?? string.Empty, 
                    Notes = _notes.Where(n => n.BookId == note.BookId).ToList(),
                    NewNote = note
                };
                return View("Notes", viewModel);
            }
            
            note.CreatedAt = DateTime.Now;
            
            if (note.Id == 0)
            {
                note.Id = _notes.Count > 0 ? _notes.Max(n => n.Id) + 1 : 1;
                _notes.Add(note);
            }
            else
            {
                var existingNote = _notes.FirstOrDefault(n => n.Id == note.Id);
                if (existingNote != null)
                {
                    existingNote.Title = note.Title;
                    existingNote.Content = note.Content;
                    existingNote.Citations = note.Citations;
                    existingNote.Links = note.Links;
                }
            }
            
            _logger.LogInformation($"Saved note {note.Id} for book {note.BookId}");
            
            return RedirectToAction("Notes", new { bookId = note.BookId });
        }
    }
}
