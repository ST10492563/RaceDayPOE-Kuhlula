
using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Models;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiClient _api;

        public OrganiserController(ApiClient api)
        {
            _api = api;
        }

        private IActionResult? CheckOrganiser()
        {
            if (string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (HttpContext.Session.GetString("Role") != "Organiser")
            {
                return Forbid();
            }

            return null;
        }

        // GET: Organiser/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var access = CheckOrganiser();

            if (access != null)
                return access;

            try
            {
                var events = await _api.GetJsonAsync<
                    List<EventViewModel>>("api/Events");

                return View(events ?? new List<EventViewModel>());
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Unable to load dashboard.";

                return View(new List<EventViewModel>());
            }
        }

        // GET: Organiser/Results/1
        [HttpGet]
        public async Task<IActionResult> Results(int id)
        {
            var access = CheckOrganiser();

            if (access != null)
                return access;

            try
            {
                var eventItem = await _api.GetJsonAsync<
                    EventViewModel>($"api/Events/{id}");

                if (eventItem == null)
                    return NotFound();

                var enrollments = await _api.GetJsonAsync<
                    List<EnrollmentViewModel>>(
                    $"api/Enrollments/event/{id}");

                var results = await _api.GetJsonAsync<
                    List<ResultViewModel>>(
                    $"api/Results/{id}");

                ViewBag.Event = eventItem;

                ViewBag.Enrollments = enrollments ??
                    new List<EnrollmentViewModel>();

                ViewBag.Results = results ??
                    new List<ResultViewModel>();

                return View(new ResultViewModel
                {
                    ResultStatus = "Finished"
                });
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Unable to load event participants or results.";

                return RedirectToAction(nameof(Dashboard));
            }
        }

        // POST: Organiser/AddResult
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddResult(
            int eventId,
            ResultViewModel model)
        {
            var access = CheckOrganiser();

            if (access != null)
                return access;

            if (!ModelState.IsValid || eventId <= 0)
            {
                TempData["Error"] =
                    "Please provide valid result information.";

                return RedirectToAction(
                    nameof(Results),
                    new { id = eventId });
            }

            try
            {
                using var response = await _api.PostJsonAsync(
                    "api/Results",
                    new
                    {
                        model.EnrollmentID,
                        model.FinishTime,
                        model.Position,
                        model.AveragePace,
                        model.ResultStatus
                    });

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] =
                        await response.Content.ReadAsStringAsync();
                }
                else
                {
                    TempData["Success"] =
                        "Race result added successfully.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "The API connection failed.";
            }

            return RedirectToAction(
                nameof(Results),
                new { id = eventId });
        }
    }
}

