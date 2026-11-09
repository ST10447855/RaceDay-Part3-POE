using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class RaceEventsController : Controller
    {
        private readonly ApiService _api;

        public RaceEventsController(ApiService api)
        {
            _api = api;
        }

        // /RaceEvents now shows the same filtered listing as the home page
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        // Full event information and available categories
        public async Task<IActionResult> Details(int id)
        {
            var result = await _api.GetAsync<EventDetailVm>($"api/public/events/{id}");
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Error;
                return RedirectToAction("Index", "Home");
            }

            return View(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> Enrol(int eventId, int? categoryId)
        {
            if (!categoryId.HasValue)
            {
                TempData["Error"] = "Please choose a category before enrolling.";
                return RedirectToAction("Details", new { id = eventId });
            }

            var result = await _api.PostAsync("api/participant/enrol", new { eventId, categoryId = categoryId.Value });

            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                return RedirectToAction("Details", new { id = eventId });
            }

            TempData["Success"] = "You are enrolled! Your enrolment is listed below.";
            return RedirectToAction("Enrolments", "Participant");
        }
    }
}
