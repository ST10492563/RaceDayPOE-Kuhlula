
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDayMvc.Models;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiClient _api;

        public AuthController(ApiClient api)
        {
            _api = api;
        }

        // REGISTER PAGE
        [HttpGet]
        public IActionResult Register()
        {
            if (IsLoggedIn())
                return RedirectToAction("Index", "Home");

            return View(new RegisterViewModel());
        }

        // REGISTER PARTICIPANT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var nameParts = model.FullName.Trim()
                .Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length < 2)
            {
                ModelState.AddModelError(
                    nameof(model.FullName),
                    "Please enter your first name and last name.");

                return View(model);
            }

            var payload = new
            {
                FirstName = nameParts[0],
                LastName = nameParts[1],
                Email = model.Email.Trim(),
                Password = model.Password,
                Phone = (string?)null,
                Role = "Participant"
            };

            try
            {
                using var response = await _api.PostJsonAsync(
                    "api/Auth/register", payload);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        await ReadApiError(response));

                    return View(model);
                }

                TempData["Success"] =
                    "Registration successful. You can now log in.";

                return RedirectToAction(nameof(Login));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Cannot connect to RaceDayApi. Make sure the API is running.");

                return View(model);
            }
        }

        // LOGIN PAGE
        [HttpGet]
        public IActionResult Login()
        {
            if (IsLoggedIn())
                return RedirectToAction("Index", "Home");

            return View(new LoginViewModel());
        }

        // LOGIN USER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using var response = await _api.PostJsonAsync(
                    "api/Auth/login",
                    new
                    {
                        Email = model.Email.Trim(),
                        Password = model.Password
                    });

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        await ReadApiError(response));

                    return View(model);
                }

                // Read the identity returned by the successful login.
                using var document = JsonDocument.Parse(
                    await response.Content.ReadAsStringAsync());

                var root = document.RootElement;

                var userId = ReadValue(
                    root, "userID", "UserID", "id", "Id");

                var role = ReadValue(root, "role", "Role");

                var email = ReadValue(root, "email", "Email");

                var fullName = ReadValue(
                    root, "fullName", "FullName", "name", "Name");

                if (string.IsNullOrWhiteSpace(userId) ||
                    string.IsNullOrWhiteSpace(role))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The API login response did not include the user ID and role.");

                    return View(model);
                }

                var normalizedRole = role.Trim();

                if (!normalizedRole.Equals(
                        "Participant",
                        StringComparison.OrdinalIgnoreCase) &&
                    !normalizedRole.Equals(
                        "Organiser",
                        StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The API returned an unrecognised user role.");

                    return View(model);
                }

                // Store the identity in the MVC session.
                HttpContext.Session.SetString("UserID", userId);
                HttpContext.Session.SetString("Role", normalizedRole);
                HttpContext.Session.SetString(
                    "Email", email ?? model.Email.Trim());
                HttpContext.Session.SetString(
                    "FullName", fullName ?? email ?? model.Email.Trim());

                TempData["Success"] = "Welcome back!";

                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Cannot connect to RaceDayApi. Make sure the API is running.");

                return View(model);
            }
            catch (JsonException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The API returned an invalid login response.");

                return View(model);
            }
        }

        // LOGOUT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                using var response = await _api.PostJsonAsync(
                    "api/Auth/logout", new { });
            }
            catch (HttpRequestException)
            {
                // Still clear the MVC session if the API is unavailable.
            }
            finally
            {
                HttpContext.Session.Clear();
                _api.ClearApiCookie();
            }

            TempData["Success"] = "You have been logged out.";

            return RedirectToAction(nameof(Login));
        }

        private bool IsLoggedIn()
        {
            return !string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("UserID"));
        }

        private static string? ReadValue(
            JsonElement element, params string[] names)
        {
            if (element.ValueKind != JsonValueKind.Object)
                return null;

            foreach (var property in element.EnumerateObject())
            {
                if (names.Any(name => string.Equals(
                    name,
                    property.Name,
                    StringComparison.OrdinalIgnoreCase)))
                {
                    return property.Value.ValueKind switch
                    {
                        JsonValueKind.String =>
                            property.Value.GetString(),

                        JsonValueKind.Number =>
                            property.Value.ToString(),

                        _ => null
                    };
                }
            }

            return null;
        }

        private static async Task<string> ReadApiError(
            System.Net.Http.HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(body))
                return $"The API request failed ({(int)response.StatusCode}).";

            try
            {
                using var doc = JsonDocument.Parse(body);

                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.String)
                    return root.GetString() ?? "The API request failed.";

                if (root.ValueKind == JsonValueKind.Object)
                {
                    foreach (var key in new[]
                    {
                        "message", "Message", "error", "title"
                    })
                    {
                        if (root.TryGetProperty(key, out var value) &&
                            value.ValueKind == JsonValueKind.String)
                        {
                            return value.GetString()
                                ?? "The API request failed.";
                        }
                    }

                    if (root.TryGetProperty(
                            "errors", out var errors) &&
                        errors.ValueKind == JsonValueKind.Object)
                    {
                        var messages = errors.EnumerateObject()
                            .SelectMany(e => e.Value.EnumerateArray())
                            .Where(v => v.ValueKind == JsonValueKind.String)
                            .Select(v => v.GetString())
                            .Where(v => !string.IsNullOrWhiteSpace(v));

                        var combined = string.Join(" ", messages);

                        if (!string.IsNullOrWhiteSpace(combined))
                            return combined;
                    }
                }
            }
            catch (JsonException)
            {
                // Handle plain-text API responses below.
            }

            return body.Length > 300 ? body[..300] : body;
        }
    }
}
