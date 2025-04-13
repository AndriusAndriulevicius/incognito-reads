using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IncognitoReads.Models;
using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace IncognitoReads.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        
        // In‑memory user storage (replace with a database in production)
        private static ConcurrentDictionary<string, RegisterViewModel> _users = new ConcurrentDictionary<string, RegisterViewModel>();

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }
        
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if username already exists
            if (_users.ContainsKey(model.Username))
            {
                ModelState.AddModelError("Username", "Username is already taken.");
                return View(model);
            }

            // Check if passwords match
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                return View(model);
            }

            // Store user (in a real app, hash the password)
            _users[model.Username] = model;

            // Create claims for authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.Email, model.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = false
                });

            return RedirectToAction("Account");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_users.TryGetValue(model.Username, out var user))
            {
                // In a real application, verify the hashed password.
                if (user.Password == model.Password)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Email, user.Email)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe
                        });

                    return RedirectToAction("Account");
                }
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }

        [HttpGet]
        public IActionResult Account()
        {
            // Ensure the user is authenticated.
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var username = User.Identity.Name ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(username) || !_users.TryGetValue(username, out var user))
            {
                _logger.LogWarning($"User not found or username is empty: {username}");
                return RedirectToAction("Login");
            }

            var model = new AccountViewModel
            {
                Name = username,
                ProfileImageUrl = "/images/profile-placeholder.jpg",
                PrimaryEmail = user?.Email ?? string.Empty,
                EmailAddresses = user?.Email != null ? new List<string> { user.Email } : new List<string>(),
                ConnectedAccounts = new List<ConnectedAccount>()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
