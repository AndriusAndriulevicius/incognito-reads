using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using IncognitoReads.Models;

namespace IncognitoReads.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly IWebHostEnvironment _env;

        // In-memory storage (for demo only)
        private static readonly List<AddBookViewModel> _allBooks   = new();
        private static readonly List<ReviewViewModel>  _allReviews = new();

        public BooksController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ─── ADD BOOK ─────────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult AddBook()
            => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddBook(AddBookViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Save uploaded cover (if any)
            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                var coversDir = Path.Combine(_env.WebRootPath, "images", "covers");
                if (!Directory.Exists(coversDir))
                    Directory.CreateDirectory(coversDir);

                var ext      = Path.GetExtension(model.CoverImage.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(coversDir, fileName);

                using var fs = new FileStream(filePath, FileMode.Create);
                model.CoverImage.CopyTo(fs);

                model.CoverImageUrl = $"/images/covers/{fileName}";
            }
            else
            {
                model.CoverImageUrl = "/images/covers/default-cover.png";
            }

            model.UserId    = User.Identity?.Name ?? "";
            model.DateAdded = DateTime.Now;
            model.Id        = _allBooks.Any() ? _allBooks.Max(b => b.Id) + 1 : 1;
            _allBooks.Add(model);

            return RedirectToAction(nameof(MyLibrary));
        }

        // ─── MY LIBRARY ────────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult MyLibrary()
        {
            var userId    = User.Identity?.Name ?? "";
            var userBooks = _allBooks
                .Where(b => b.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return View(new LibraryViewModel { Books = userBooks });
        }

        // ─── EDIT BOOK ─────────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult EditBook(int id)
        {
            var userId = User.Identity?.Name ?? "";
            var book = _allBooks.FirstOrDefault(b =>
                b.Id == id &&
                b.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase));

            if (book == null)
                return NotFound();

            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditBook(AddBookViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.Identity?.Name ?? "";
            var book = _allBooks.FirstOrDefault(b =>
                b.Id == model.Id &&
                b.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase));

            if (book == null)
                return NotFound();

            // Update fields
            book.Title       = model.Title;
            book.Author      = model.Author;
            book.Genre       = model.Genre;
            book.Description = model.Description;
            book.Color       = model.Color;
            book.Notes       = model.Notes;

            // Handle new cover upload
            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                var coversDir = Path.Combine(_env.WebRootPath, "images", "covers");
                if (!Directory.Exists(coversDir))
                    Directory.CreateDirectory(coversDir);

                var ext      = Path.GetExtension(model.CoverImage.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(coversDir, fileName);

                using var fs = new FileStream(filePath, FileMode.Create);
                model.CoverImage.CopyTo(fs);

                book.CoverImageUrl = $"/images/covers/{fileName}";
            }

            return RedirectToAction(nameof(MyLibrary));
        }

        // ─── DELETE BOOK ───────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userId = User.Identity?.Name ?? "";
            var book = _allBooks.FirstOrDefault(b =>
                b.Id == id &&
                b.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase));

            if (book != null)
                _allBooks.Remove(book);

            return RedirectToAction(nameof(MyLibrary));
        }

        // ─── BOOK DETAILS ──────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult BookDetails(int id)
        {
            var book = _allBooks.FirstOrDefault(b => b.Id == id);
            if (book == null)
                return NotFound();

            // Load reviews for this book
            var reviewsForBook = _allReviews
                .Where(r => r.BookId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            // Optionally, you could pass both Book and Reviews in a combined ViewModel.
            ViewBag.Reviews = reviewsForBook;
            return View(book);
        }

        // ─── WRITE A REVIEW ────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Review()
        {
            return View(new ReviewViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitReview(ReviewViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Review", model);

            // 1) Save cover image from review (to use for the new book)
            string coverUrl;
            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                var coversDir = Path.Combine(_env.WebRootPath, "images", "covers");
                if (!Directory.Exists(coversDir))
                    Directory.CreateDirectory(coversDir);

                var ext      = Path.GetExtension(model.CoverImage.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(coversDir, fileName);

                using var fs = new FileStream(filePath, FileMode.Create);
                model.CoverImage.CopyTo(fs);

                coverUrl = $"/images/covers/{fileName}";
            }
            else
            {
                coverUrl = "/images/covers/default-cover.png";
            }

            // 2) Create the book from the review data
            var newBook = new AddBookViewModel
            {
                Id            = _allBooks.Any() ? _allBooks.Max(b => b.Id) + 1 : 1,
                Title         = model.BookTitle,
                Genre         = model.Genre,
                CoverImageUrl = coverUrl,
                DateAdded     = DateTime.Now,
                UserId        = User.Identity?.Name ?? ""
            };
            _allBooks.Add(newBook);

            // 3) Create and store the review
            model.BookId    = newBook.Id;
            model.UserId    = User.Identity?.Name ?? "";
            model.CreatedAt = DateTime.Now;
            model.Id        = _allReviews.Any() ? _allReviews.Max(r => r.Id) + 1 : 1;
            _allReviews.Add(model);

            return RedirectToAction("BookDetails", new { id = newBook.Id });
        }
    }
}
