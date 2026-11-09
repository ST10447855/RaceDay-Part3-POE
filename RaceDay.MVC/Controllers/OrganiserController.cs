using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    [SessionAuthorize("Organiser")]
    public class OrganiserController : Controller
    {
        private readonly ApiService _api;

        public OrganiserController(ApiService api)
        {
            _api = api;
        }

        // ---------- Dashboard ----------
        public async Task<IActionResult> Index()
        {
            var result = await _api.GetAsync<List<EventVm>>("api/organiser/events");
            if (!result.Success) ViewBag.ApiError = result.Error;
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            return View(result.Data ?? new List<EventVm>());
        }

        // ---------- Events ----------
        public async Task<IActionResult> Events()
        {
            var result = await _api.GetAsync<List<EventVm>>("api/organiser/events");
            if (!result.Success) ViewBag.ApiError = result.Error;
            return View(result.Data ?? new List<EventVm>());
        }

        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            var vm = new EventFormVm { EventDate = DateTime.Today.AddDays(30).AddHours(8) };
            await LoadEventTypes(vm);
            return View("EventForm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventFormVm vm)
        {
            return await SaveEvent(vm, isNew: true);
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var events = await _api.GetAsync<List<EventVm>>("api/organiser/events");
            var ev = events.Data?.FirstOrDefault(e => e.EventID == id);
            if (ev == null)
            {
                TempData["Error"] = events.Success ? "That event could not be found." : events.Error;
                return RedirectToAction("Events");
            }

            var vm = new EventFormVm
            {
                EventID = ev.EventID,
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                Distance = ev.Distance,
                EventTypeID = ev.EventTypeID,
                BannerImageURL = ev.BannerImageURL
            };
            await LoadEventTypes(vm);
            return View("EventForm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(EventFormVm vm)
        {
            return await SaveEvent(vm, isNew: false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var result = await _api.DeleteAsync($"api/organiser/events/{id}");
            if (result.Success) TempData["Success"] = "Event deleted.";
            else TempData["Error"] = result.Error;
            return RedirectToAction("Events");
        }

        private async Task<IActionResult> SaveEvent(EventFormVm vm, bool isNew)
        {
            if (!ModelState.IsValid)
            {
                await LoadEventTypes(vm);
                return View("EventForm", vm);
            }

            var bannerUrl = vm.BannerImageURL;
            if (vm.Banner != null && vm.Banner.Length > 0)
            {
                var upload = await _api.UploadImageAsync(vm.Banner);
                if (!upload.Success || string.IsNullOrWhiteSpace(upload.Data?.Url))
                {
                    ModelState.AddModelError("", upload.Error.Length > 0 ? upload.Error : "The banner could not be uploaded.");
                    await LoadEventTypes(vm);
                    return View("EventForm", vm);
                }
                bannerUrl = upload.Data!.Url;
            }

            var body = new
            {
                vm.Name,
                vm.Description,
                vm.EventDate,
                vm.Location,
                vm.Distance,
                vm.EventTypeID,
                BannerImageURL = bannerUrl
            };

            var result = isNew
                ? await _api.PostAsync("api/organiser/events", body)
                : await _api.PutAsync($"api/organiser/events/{vm.EventID}", body);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error);
                await LoadEventTypes(vm);
                return View("EventForm", vm);
            }

            TempData["Success"] = isNew ? "Event created." : "Event updated.";
            return RedirectToAction("Events");
        }

        private async Task LoadEventTypes(EventFormVm vm)
        {
            var types = await _api.GetAsync<List<EventTypeVm>>("api/EventTypes");
            vm.EventTypes = types.Data ?? new List<EventTypeVm>();
        }

        // ---------- Categories ----------
        public async Task<IActionResult> Categories(int? eventId)
        {
            var page = await BuildPage<CategoryVm>(eventId, "categories");
            return View(page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(CategoryFormVm vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage
                                    ?? "Please check the category details.";
                return RedirectToAction("Categories", new { eventId = vm.EventID });
            }

            var result = await _api.PostAsync($"api/organiser/events/{vm.EventID}/categories", new
            {
                vm.CategoryName,
                vm.MinimumAge,
                vm.MaximumAge,
                vm.Distance
            });

            if (result.Success) TempData["Success"] = "Category added.";
            else TempData["Error"] = result.Error;
            return RedirectToAction("Categories", new { eventId = vm.EventID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id, int eventId)
        {
            var result = await _api.DeleteAsync($"api/organiser/categories/{id}");
            if (result.Success) TempData["Success"] = "Category removed.";
            else TempData["Error"] = result.Error;
            return RedirectToAction("Categories", new { eventId });
        }

        // ---------- Enrolments ----------
        public async Task<IActionResult> Enrolments(int? eventId)
        {
            var page = await BuildPage<OrganiserEnrolmentVm>(eventId, "enrolments");
            return View(page);
        }

        // ---------- Results ----------
        public async Task<IActionResult> Results(int? eventId)
        {
            var page = await BuildPage<OrganiserEnrolmentVm>(eventId, "enrolments");
            return View(page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(int enrolmentId, int eventId, string? finishingTime, int? position)
        {
            if (string.IsNullOrWhiteSpace(finishingTime) || !position.HasValue || position.Value < 1)
            {
                TempData["Error"] = "Enter a finishing time (e.g. 00:52:14) and a position of 1 or more.";
                return RedirectToAction("Results", new { eventId });
            }

            var result = await _api.PostAsync($"api/organiser/enrolments/{enrolmentId}/result", new
            {
                FinishingTime = finishingTime.Trim(),
                Position = position.Value
            });

            if (result.Success) TempData["Success"] = "Result saved.";
            else TempData["Error"] = result.Error;
            return RedirectToAction("Results", new { eventId });
        }

        // Loads the organiser's events (for the picker) plus the rows for the selected event
        private async Task<OrganiserEventPageVm<T>> BuildPage<T>(int? eventId, string suffix)
        {
            var events = await _api.GetAsync<List<EventVm>>("api/organiser/events");
            var page = new OrganiserEventPageVm<T> { Events = events.Data ?? new List<EventVm>() };
            if (!events.Success) ViewBag.ApiError = events.Error;

            page.SelectedEventId = eventId ?? page.Events.FirstOrDefault()?.EventID;

            if (page.SelectedEventId.HasValue)
            {
                var items = await _api.GetAsync<List<T>>($"api/organiser/events/{page.SelectedEventId.Value}/{suffix}");
                if (items.Success) page.Items = items.Data ?? new List<T>();
                else ViewBag.ApiError = items.Error;
            }

            return page;
        }
    }
}
