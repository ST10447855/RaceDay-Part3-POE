using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _api;

        public HomeController(ApiService api)
        {
            _api = api;
        }

        // Home page: all upcoming events with search + event-type filtering
        public async Task<IActionResult> Index(string? search, int? typeId)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) query.Add("search=" + Uri.EscapeDataString(search.Trim()));
            if (typeId.HasValue) query.Add("eventTypeId=" + typeId.Value);

            var url = "api/public/events" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

            var events = await _api.GetAsync<List<EventVm>>(url);
            var types = await _api.GetAsync<List<EventTypeVm>>("api/EventTypes");

            if (!events.Success) ViewBag.ApiError = events.Error;

            return View(new HomeIndexVm
            {
                Events = events.Data ?? new List<EventVm>(),
                Types = types.Data ?? new List<EventTypeVm>(),
                Search = search,
                TypeId = typeId
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
