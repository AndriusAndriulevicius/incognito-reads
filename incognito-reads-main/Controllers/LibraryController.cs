using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IncognitoReads.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IncognitoReads.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        // In-memory storage for demo purposes.
        private static List<AddBookViewModel> _userBooks = new List<AddBookViewModel>();

        // GET: /Library/Index (My Library)
        [HttpGet]
        public IActionResult Index()
        {
            // Retrieve the current user's ID.
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "TestUser";
            var model = new LibraryViewModel
            {
                Books = _userBooks.Where(b => b.UserId == currentUserId).ToList()
            };
            System.Diagnostics.Debug.WriteLine($"[Index] User: {currentUserId}, Books Count: {model.Books.Count}");
            return View(model);  // Looks for Views/Library/Index.cshtml
        }

        // GET: /Library/AddBook
        [HttpGet]
        public IActionResult AddBook()
        {
            return View(new AddBookViewModel());
        }

        // POST: /Library/AddBook
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddBook(AddBookViewModel book)
        {
            if (ModelState.IsValid)
            {
                string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "TestUser";
                book.Id = _userBooks.Any() ? _userBooks.Max(b => b.Id) + 1 : 1;
                book.DateAdded = DateTime.UtcNow;
                book.UserId = currentUserId;
                _userBooks.Add(book);
                System.Diagnostics.Debug.WriteLine($"[AddBook POST] Added book for user {currentUserId}. Total books now: {_userBooks.Count}");
                return RedirectToAction("Index");
            }
            return View(book);
        }

        
    }
}
