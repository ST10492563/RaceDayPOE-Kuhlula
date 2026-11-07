
using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Models;
using RaceDayMvc.Services;
using System.Net;

namespace RaceDayMvc.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApiClient _api;

        public EventsController(ApiClient api)
        {
            _api = api;
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("Role")
                == "Organiser";
        }

        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(
                HttpContext.Session.GetString("UserID"));
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            try
            {
                var events = await _api.GetJsonAsync<
                    List<EventViewModel>>("api/Events");

                return View(events ?? new List<EventViewModel>());
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Unable to connect to the API.";
                return View(new List<EventViewModel>());
            }
        }

        // GET: Events/Details/1
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var eventItem = await _api.GetJsonAsync<
                    EventViewModel>($"api/Events/{id}");

                if (eventItem == null)
                    return NotFound();

                return View(eventItem);
            }
            catch (HttpRequestException)
            {
                return NotFound();
            }
        }

        // GET: Events/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsLoggedIn() || !IsOrganiser())
                return RedirectToAction("Login", "Auth");

            return View(new EventViewModel
            {
                EventDate = DateTime.Today,
                Status = "Upcoming"
            });
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            EventViewModel model)
        {
            if (!IsLoggedIn() || !IsOrganiser())
                return Unauthorized();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using var response = await _api.PostJsonAsync(
                    "api/Events", model);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("",
                        await response.Content.ReadAsStringAsync());

                    return View(model);
                }

                TempData["Success"] = "Event created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError("",
                    "Could not connect to the API.");

                return View(model);
            }
        }

        // GET: Events/Edit/1
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsLoggedIn() || !IsOrganiser())
                return Unauthorized();

            try
            {
                var eventItem = await _api.GetJsonAsync<
                    EventViewModel>($"api/Events/{id}");

                if (eventItem == null)
                    return NotFound();

                return View(eventItem);
            }
            catch (HttpRequestException)
            {
                return NotFound();
            }
        }

        // POST: Events/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, EventViewModel model)
        {
            if (!IsLoggedIn() || !IsOrganiser())
                return Unauthorized();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using var response = await _api.PutJsonAsync(
                    $"api/Events/{id}", model);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("",
                        await response.Content.ReadAsStringAsync());

                    return View(model);
                }

                TempData["Success"] = "Event updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError("",
                    "Could not connect to the API.");

                return View(model);
            }
        }

        // POST: Events/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsLoggedIn() || !IsOrganiser())
                return Unauthorized();

            try
            {
                using var response = await _api.DeleteAsync(
                    $"api/Events/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] =
                        await response.Content.ReadAsStringAsync();
                }
                else
                {
                    TempData["Success"] =
                        "Event deleted successfully.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Could not connect to the API.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
