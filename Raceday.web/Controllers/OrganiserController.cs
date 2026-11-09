using Microsoft.AspNetCore.Mvc;
using Raceday.web.Filters;
using Raceday.web.Models;
using Raceday.web.Services;

namespace Raceday.web.Controllers
{
    [AuthorizeRole("Organiser")]
    public class OrganiserController : Controller
    {
        private readonly ApiService _apiService;

        public OrganiserController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Dashboard()
        {
            int organizerId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var enrolments = await _apiService.GetOrganizerEnrolmentsAsync(organizerId);
            var events = await _apiService.GetAllEventsAsync();

            ViewBag.TotalEnrolments = enrolments.Count;
            ViewBag.TotalEvents = events.Count;
            return View(events);
        }

        public async Task<IActionResult> Events()
        {
            int organizerId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var allEvents = await _apiService.GetAllEventsAsync();

            // Filter events where OrganizerId matches the current logged-in session user ID
            var organiserEvents = allEvents.Where(e => e.OrganizerId == organizerId).ToList();

            return View(organiserEvents);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventViewModel model)
        {
            // Automatically bind the current logged-in user's ID as the OrganizerId
            model.OrganizerId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (await _apiService.CreateEventAsync(model, model.BannerImageFile))
            {
                TempData["Success"] = "Event created successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to create event.";
            }
            return RedirectToAction("Events");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            int organizerId = HttpContext.Session.GetInt32("UserId") ?? 0;
            await _apiService.DeleteEventAsync(eventId, organizerId);
            return RedirectToAction("Events");
        }

        [HttpGet]
        public async Task<IActionResult> Categories(int eventId)
        {
            // Ensure ViewBag receives the eventId so the hidden input can pick it up
            ViewBag.EventId = eventId;

            // Fetch existing categories for this specific event
            var categories = await _apiService.GetCategoriesByEventAsync(eventId);

            return View(categories);
        }


        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryViewModel model)
        {
            // If model.EventId is 0 here, it means the hidden input wasn't bound correctly
            if (model.EventId <= 0)
            {
                ModelState.AddModelError("", "Event ID is missing.");
                ViewBag.EventId = model.EventId;
                return View("Categories", await _apiService.GetCategoriesByEventAsync(model.EventId));
            }

            if (await _apiService.CreateCategoryAsync(model))
            {
                TempData["Success"] = "Category added.";
            }

            // Redirect back passing the exact eventId
            return RedirectToAction("Categories", new { eventId = model.EventId });
        }



        public async Task<IActionResult> Enrolments()
        {
            int organizerId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var enrolments = await _apiService.GetOrganizerEnrolmentsAsync(organizerId);
            return View(enrolments);
        }



        [HttpGet]
        public IActionResult CaptureResult(int enrolmentId)
        {
            return View(new ResultViewModel { EnrolmentId = enrolmentId });
        }

        [HttpPost]
        public async Task<IActionResult> CaptureResult(ResultViewModel model)
        {
            if (await _apiService.CaptureResultAsync(model))
            {
                TempData["Success"] = "Result published!";
                return RedirectToAction("Enrolments");
            }
            ModelState.AddModelError("", "Failed to record race result.");
            return View(model);
        }
    }
}
