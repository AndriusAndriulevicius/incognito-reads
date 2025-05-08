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

            var model = new AccountViewModel
            {
                Name = username,
                ProfileImageUrl = imageUrl,
                PrimaryEmail = user.Email,
                EmailAddresses = new List<string> { user.Email },
                ConnectedAccounts = new List<ConnectedAccount>()
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
    }
}
