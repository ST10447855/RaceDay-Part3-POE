
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public AccountController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Please enter your email and password.");
                return View();
            }

            var client = _clientFactory.CreateClient("RaceDayAPI");

            var loginData = new
            {
                email,
                password
            };

            try
            {
                var response = await client.PostAsJsonAsync(
                    "api/Auth/login", loginData);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View();
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result == null || string.IsNullOrWhiteSpace(result.Token))
                {
                    ModelState.AddModelError("", "Login failed. Please try again.");
                    return View();
                }

                HttpContext.Session.SetString("JWTToken", result.Token);
                HttpContext.Session.SetString("UserEmail", result.Email ?? email);
                HttpContext.Session.SetString("UserRole", result.Role ?? "");
                HttpContext.Session.SetString("UserId", result.UserId.ToString());

                if (string.Equals(result.Role, "Organiser", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Organiser");

                if (string.Equals(result.Role, "Participant", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Participant");

                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError("", "Cannot connect to RaceDay API.");
                return View();
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string firstName,
            string lastName,
            string email,
            string password,
            string? phoneNumber,
            string? role)
        {
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Please complete all required fields.");
                return View();
            }

            var client = _clientFactory.CreateClient("RaceDayAPI");

            var registrationData = new
            {
                firstName,
                lastName,
                email,
                password,
                phoneNumber,
                role
            };

            try
            {
                var response = await client.PostAsJsonAsync(
                    "api/Auth/register", registrationData);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var friendly = "Registration failed. Please check your details and try again.";
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(body);
                        if (doc.RootElement.TryGetProperty("message", out var msg) &&
                            msg.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            friendly = msg.GetString() ?? friendly;
                        }
                    }
                    catch (System.Text.Json.JsonException) { }

                    ModelState.AddModelError("", friendly);

                    return View();
                }

                TempData["SuccessMessage"] =
                    "Registration successful. You can now log in.";

                return RedirectToAction("Login");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError("", "Cannot connect to RaceDay API.");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private class LoginResponse
        {
            public string? Token { get; set; }
            public string? Email { get; set; }
            public string? Role { get; set; }
            public int UserId { get; set; }
        }
    }
}
