using Microsoft.AspNetCore.Mvc;
using Raceday.web.Models;
using Raceday.web.Services;
using System.Threading.Tasks;
namespace Raceday.web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;

        public AccountController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, user, message) = await _apiService.LoginAsync(model.Email, model.Password);

            if (success && user != null)
            {
                // Store user session details
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserName", user.Fullname);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role);

                // Role-based redirection to the appropriate dashboard
                if (!string.IsNullOrEmpty(user.Role) && user.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "Organiser");
                }
                return RedirectToAction("Index", "Participant");
            }

            ModelState.AddModelError(string.Empty, message ?? "Invalid email or password.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(UserRegistrationViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, user, message) = await _apiService.RegisterAsync(model);

            if (success && user != null)
            {
                // Automatically set session on successful registration
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserName", user.Fullname);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role);

                TempData["SuccessMessage"] = "Account successfully created!";

                // Redirect based on user role
                if (!string.IsNullOrEmpty(user.Role) && user.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "Organiser");
                }
                return RedirectToAction("Index", "Participant");
            }

            ModelState.AddModelError(string.Empty, message ?? "Registration failed. Please try again.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}