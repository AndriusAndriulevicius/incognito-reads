using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IncognitoReads.Models;

namespace IncognitoReads.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        // In-memory storage (replace with database in production)
        private static List<BookViewModel> _userBooks = new List<BookViewModel>();

        [HttpGet]
        public IActionResult Index()
        {
            var model = new LibraryViewModel
            {
                Books = _userBooks
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult AddBook()
        {
            return View(new BookViewModel());
        }

        [HttpPost]
        public IActionResult AddBook(BookViewModel book)
        {
            if (ModelState.IsValid)
            {
                book.Id = _userBooks.Count + 1;
                book.DateAdded = DateTime.UtcNow;
                _userBooks.Add(book);
                return RedirectToAction("Index");
            }
            return View(book);
        }
    }
}