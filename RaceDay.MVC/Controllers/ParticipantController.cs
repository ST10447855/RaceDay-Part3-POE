using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    [SessionAuthorize("Participant")]
    public class ParticipantController : Controller
    {
        private readonly ApiService _api;

        public ParticipantController(ApiService api)
        {
            _api = api;
        }

        public IActionResult Index()
        {
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            return View();
        }

        // My enrolments with colour-coded status
        public async Task<IActionResult> Enrolments()
        {
            var result = await _api.GetAsync<List<EnrolmentVm>>("api/participant/enrolments");
            if (!result.Success) ViewBag.ApiError = result.Error;
            return View(result.Data ?? new List<EnrolmentVm>());
        }

        // My race history: time and position
        public async Task<IActionResult> Results()
        {
            var result = await _api.GetAsync<List<ResultVm>>("api/participant/results");
            if (!result.Success) ViewBag.ApiError = result.Error;
            return View(result.Data ?? new List<ResultVm>());
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var result = await _api.GetAsync<ProfileVm>("api/participant/profile");
            if (!result.Success || result.Data == null)
            {
                ViewBag.ApiError = result.Error;
                return View(new ProfileVm());
            }

            var profile = result.Data;
            // A profile that was never completed comes back with an empty/default date of birth
            if (profile.DateOfBirth.HasValue && profile.DateOfBirth.Value.Year < 1900)
            {
                profile.DateOfBirth = null;
            }
            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileVm model, IFormFile? profilePicture)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var pictureUrl = model.ProfilePictureURL;

            if (profilePicture != null && profilePicture.Length > 0)
            {
                var upload = await _api.UploadImageAsync(profilePicture);
                if (!upload.Success || string.IsNullOrWhiteSpace(upload.Data?.Url))
                {
                    ModelState.AddModelError("", upload.Error.Length > 0 ? upload.Error : "The picture could not be uploaded.");
                    return View(model);
                }
                pictureUrl = upload.Data!.Url;
            }

            var save = await _api.PutAsync("api/participant/profile", new
            {
                model.FirstName,
                model.LastName,
                model.PhoneNumber,
                model.DateOfBirth,
                ProfilePictureURL = pictureUrl
            });

            if (!save.Success)
            {
                ModelState.AddModelError("", save.Error);
                return View(model);
            }

            TempData["Success"] = "Your profile has been updated.";
            return RedirectToAction("Profile");
        }
    }
}
