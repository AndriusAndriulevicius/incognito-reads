using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IncognitoReads.Models;
using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IncognitoReads.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IWebHostEnvironment _env;

        // In-memory user storage (replace with a database in production)
        private static ConcurrentDictionary<string, RegisterViewModel> _users
            = new ConcurrentDictionary<string, RegisterViewModel>();

        // Atm. in-memory profilio nuotraukų žemėlapis
        private static ConcurrentDictionary<string, string> _profileImages
            = new ConcurrentDictionary<string, string>();

        // New dictionaries for user preferences
        private static ConcurrentDictionary<string, string> _favoriteGenres
            = new ConcurrentDictionary<string, string>();
            
        private static ConcurrentDictionary<string, List<string>> _favoriteBooks
            = new ConcurrentDictionary<string, List<string>>();

        public AccountController(ILogger<AccountController> logger,
                                 IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        [HttpGet]
        public IActionResult Register()
            => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_users.ContainsKey(model.Username))
            {
                ModelState.AddModelError("Username", "Username is already taken.");
                return View(model);
            }
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                return View(model);
            }

            // Nustatome default profilio nuotrauką
            model.ProfileImageUrl = "/images/profile-placeholder.jpg";
            _users[model.Username] = model;
            
            // Initialize empty favorite books list for new user
            _favoriteBooks[model.Username] = new List<string>();

            // Sukuriame claims ir prisijungiame
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.Email, model.Email)
            };
            var ci = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(ci),
                new AuthenticationProperties { IsPersistent = false });

            return RedirectToAction("Account");
        }

        [HttpGet]
        public IActionResult Login()
            => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_users.TryGetValue(model.Username, out var user)
                && user.Password == model.Password)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email)
                };
                var ci = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(ci),
                    new AuthenticationProperties { IsPersistent = model.RememberMe });

                return RedirectToAction("Account");
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }

        [HttpGet]
        public IActionResult Account()
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
                return RedirectToAction("Login");

            var username = User.Identity.Name ?? "";
            if (!_users.TryGetValue(username, out var user))
            {
                _logger.LogWarning($"User not found: {username}");
                return RedirectToAction("Login");
            }

            // Paimame įkeltą nuotrauką iš žemėlapio arba default
            var imageUrl = _profileImages.TryGetValue(username, out var url)
                           ? url
                           : user.ProfileImageUrl;

            // Get user's favorite genre if exists
            var favoriteGenre = _favoriteGenres.TryGetValue(username, out var genre)
                               ? genre
                               : string.Empty;
                               
            // Get user's favorite books or empty list
            var favoriteBooks = _favoriteBooks.TryGetValue(username, out var books)
                               ? books
                               : new List<string>();

            var model = new AccountViewModel
            {
                Name = username,
                ProfileImageUrl = imageUrl,
                PrimaryEmail = user.Email,
                EmailAddresses = new List<string> { user.Email },
                ConnectedAccounts = new List<ConnectedAccount>(),
                FavoriteGenre = favoriteGenre,
                FavoriteBooks = favoriteBooks
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // Naujas veiksmas profilio nuotraukai priimti
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePicture(IFormFile ProfileImage)
        {
            if (ProfileImage != null && ProfileImage.Length > 0
                && User?.Identity?.IsAuthenticated == true)
            {
                var username = User.Identity.Name!;
                // Sukuriame katalogą wwwroot/images/profiles jei jo nėra
                var profilesDir = Path.Combine(_env.WebRootPath, "images", "profiles");
                if (!Directory.Exists(profilesDir))
                    Directory.CreateDirectory(profilesDir);

                // Unikalus failo vardas: username + ext
                var ext = Path.GetExtension(ProfileImage.FileName);
                var fileName = $"{username}{ext}";
                var filePath = Path.Combine(profilesDir, fileName);

                // Išsaugome faile
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await ProfileImage.CopyToAsync(stream);

                // Įrašome viešą URL
                var publicUrl = $"/images/profiles/{fileName}";
                _profileImages[username] = publicUrl;
            }

            return RedirectToAction("Account");
        }
        
        // New action to update reading preferences
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePreferences(string FavoriteGenre, string NewFavoriteBook)
        {
            if (User?.Identity?.IsAuthenticated != true)
                return RedirectToAction("Login");
                
            var username = User.Identity.Name!;
            
            // Update favorite genre if provided
            if (!string.IsNullOrWhiteSpace(FavoriteGenre))
            {
                _favoriteGenres[username] = FavoriteGenre;
            }
            
            // Add new favorite book if provided
            if (!string.IsNullOrWhiteSpace(NewFavoriteBook))
            {
                // Initialize list if it doesn't exist
                if (!_favoriteBooks.TryGetValue(username, out var books))
                {
                    books = new List<string>();
                    _favoriteBooks[username] = books;
                }
                
                // Add book if it doesn't already exist in the list
                if (!books.Contains(NewFavoriteBook))
                {
                    books.Add(NewFavoriteBook);
                }
            }
            
            return RedirectToAction("Account");
        }
        
        // Action to remove a favorite book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFavoriteBook(string bookTitle)
        {
            if (User?.Identity?.IsAuthenticated != true)
                return RedirectToAction("Login");
                
            var username = User.Identity.Name!;
            
            if (!string.IsNullOrWhiteSpace(bookTitle) && 
                _favoriteBooks.TryGetValue(username, out var books))
            {
                books.Remove(bookTitle);
            }
            
            return RedirectToAction("Account");
        }
    }
}