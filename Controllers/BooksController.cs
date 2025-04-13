using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IncognitoReads.Models;

namespace IncognitoReads.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        // In‑memory collection for demo purposes.
        // In production, use a database.
        private static List<AddBookViewModel> _allBooks = new List<AddBookViewModel>();

        // In‑memory collection for reviews.
        private static List<ReviewViewModel> _allReviews = new List<ReviewViewModel>();

        [HttpGet]
        public IActionResult AddBook()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddBook(AddBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.UserId = User.Identity?.Name ?? "";
            model.DateAdded = DateTime.Now;
            model.Id = _allBooks.Count > 0 ? _allBooks.Max(b => b.Id) + 1 : 1;
            _allBooks.Add(model);

            return RedirectToAction("MyLibrary");
        }

        [HttpGet]
        public IActionResult MyLibrary()
        {
            var userId = User.Identity?.Name ?? "";
            var userBooks = _allBooks
                .Where(b => b.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var viewModel = new LibraryViewModel { Books = userBooks };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult EditBook(int id)
        {
            var userId = User.Identity?.Name ?? "";
            var book = _allBooks.FirstOrDefault(b => b.Id == id && b.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase));
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditBook(AddBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity?.Name ?? "";
            var book = _allBooks.FirstOrDefault(b => b.Id == model.Id && b.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase));
            if (book == null)
            {
                return NotFound();
            }

            book.Title = model.Title;
            book.Author = model.Author;
            book.Genre = model.Genre;
            book.Description = model.Description;
            book.Color = model.Color;

            if (model.CoverImage != null)
            {
                // Image upload handling would occur here in a real app.
                book.CoverImage = model.CoverImage;
            }

            return RedirectToAction("MyLibrary");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userId = User.Identity?.Name ?? "";
            var book = _allBooks.FirstOrDefault(b => b.Id == id && b.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase));
            if (book != null)
            {
                _allBooks.Remove(book);
                _allReviews.RemoveAll(r => r.BookId == book.Id);
            }
            return RedirectToAction("MyLibrary");
        }

        [HttpGet]
        public IActionResult BookDetails(int id)
        {
            var book = _allBooks.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            var reviewsForBook = _allReviews
                .Where(r => r.BookId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            var viewModel = new BookDetailsViewModel
            {
                Book = book,
                Reviews = reviewsForBook,
                NewReview = new ReviewViewModel { BookId = book.Id }
            };

            return View(viewModel);
        }

        // Existing AddReview used on the BookDetails page remains unchanged.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(ReviewViewModel model)
        {
            var userId = User.Identity?.Name ?? "";
            var book = _allBooks.FirstOrDefault(b => b.Id == model.BookId);
            if (book == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var reviewsForBook = _allReviews
                    .Where(r => r.BookId == model.BookId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();

                var viewModel = new BookDetailsViewModel
                {
                    Book = book,
                    Reviews = reviewsForBook,
                    NewReview = model
                };

                return View("BookDetails", viewModel);
            }

            model.UserId = userId;
            model.CreatedAt = DateTime.Now;
            model.BookId = _allReviews.Count > 0 ? _allReviews.Max(r => r.BookId) + 1 : 1;
            _allReviews.Add(model);

            return RedirectToAction("BookDetails", new { id = model.BookId });
        }

        // New GET action to serve the review form page.
        [HttpGet]
        public IActionResult Review()
        {
            return View();
        }

        // New POST action to handle submission from the standalone review page.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitReview(ReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Review", model);
            }

            // Create a new book record using the review's book details.
            var newBook = new AddBookViewModel
            {
                Id = _allBooks.Count > 0 ? _allBooks.Max(b => b.Id) + 1 : 1,
                Title = model.BookTitle,
                Genre = model.Genre,
                CoverImage = model.CoverImage,
                DateAdded = DateTime.Now,
                UserId = User.Identity?.Name ?? ""
            };
            _allBooks.Add(newBook);

            // Associate the review with the newly created book.
            model.BookId = newBook.Id;
            model.UserId = User.Identity?.Name ?? "";
            model.CreatedAt = DateTime.Now;
            model.BookId = _allReviews.Count > 0 ? _allReviews.Max(r => r.BookId) + 1 : 1;
            _allReviews.Add(model);

            // After submission, redirect the user to view the book details along with its review.
            return RedirectToAction("BookDetails", new { id = newBook.Id });
        }
    }
}
