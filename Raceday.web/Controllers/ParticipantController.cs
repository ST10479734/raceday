using Microsoft.AspNetCore.Mvc;
using Raceday.web.Filters;
using Raceday.web.Models;
using Raceday.web.Services;

namespace Raceday.web.Controllers
{
    [AuthorizeRole("Participant")]
    public class ParticipantController : Controller
    {
        private readonly ApiService _apiService;

        public ParticipantController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index(string? search, string? eventType)
        {
            var events = await _apiService.GetAllEventsAsync();
            if (!string.IsNullOrEmpty(search))
            {
                events = events.Where(e => e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(eventType))
            {
                events = events.Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            return View(events);
        }

        public async Task<IActionResult> EventDetails(int id)
        {
            var categories = await _apiService.GetCategoriesByEventAsync(id);
            var eventModel = new EventViewModel
            {
                EventId = id,
                EventName = "Race Event",
                Categories = categories
            };
            return View(eventModel);
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Enrol(int categoryId, decimal amount)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            bool success = await _apiService.CreateEnrolmentAsync(userId, categoryId, amount);
            if (success)
            {
                TempData["Success"] = "Successfully enrolled in the event!";
            }
            else
            {
                TempData["Error"] = "Enrolment failed. Please try again.";
            }

            return RedirectToAction("MyEnrolments");
        }

        public async Task<IActionResult> MyEnrolments()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var results = await _apiService.GetMyResultsAsync(userId);
            return View(results);
        }

        public async Task<IActionResult> MyResults()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var results = await _apiService.GetMyResultsAsync(userId);
            return View(results);
        }

        public IActionResult Profile()
        {
            var user = new UserSessionModel
            {
                Fullname = HttpContext.Session.GetString("UserName") ?? "",
                Email = HttpContext.Session.GetString("UserEmail") ?? "",
                Role = HttpContext.Session.GetString("UserRole") ?? ""
            };
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullname, string password, IFormFile? profilePicture)
        {
            string email = HttpContext.Session.GetString("UserEmail") ?? "";
            string role = HttpContext.Session.GetString("UserRole") ?? "Participant";

            if (await _apiService.UpdateProfileAsync(email, fullname, role, password, profilePicture))
            {
                HttpContext.Session.SetString("UserName", fullname);
                TempData["Success"] = "Profile updated successfully!";
            }
            return RedirectToAction("Profile");
        }
    }
}
