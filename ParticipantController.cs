
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDayMvc.Models;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiClient _api;

        public ParticipantController(ApiClient api)
        {
            _api = api;
        }

        // Check whether the logged-in user is a participant.
        private IActionResult? CheckAccess()
        {
            var userId = HttpContext.Session.GetString("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (role != "Participant")
            {
                return Forbid();
            }

            return null;
        }

        // GET: Participant/Enroll/1
        [HttpGet]
        public async Task<IActionResult> Enroll(int id)
        {
            var access = CheckAccess();

            if (access != null)
                return access;

            try
            {
                var categories =
                    await _api.GetJsonAsync<List<CategoryViewModel>>(
                        "api/Categories");

                ViewBag.Categories =
                    categories ?? new List<CategoryViewModel>();

                var eventItem =
                    await _api.GetJsonAsync<EventViewModel>(
                        $"api/Events/{id}");

                if (eventItem == null)
                    return NotFound();

                return View(eventItem);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Unable to connect to the API.";

                return RedirectToAction("Index", "Events");
            }
        }

        // POST: Participant/Enroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(
            int eventId,
            int categoryId)
        {
            var access = CheckAccess();

            if (access != null)
                return access;

            if (eventId <= 0 || categoryId <= 0)
            {
                TempData["Error"] =
                    "Please select a valid event and category.";

                return RedirectToAction(
                    "Enroll",
                    new { id = eventId });
            }

            try
            {
                using var response = await _api.PostJsonAsync(
                    "api/Enrollments",
                    new
                    {
                        EventID = eventId,
                        CategoryID = categoryId
                    });

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] =
                        await response.Content.ReadAsStringAsync();

                    return RedirectToAction(
                        "Enroll",
                        new { id = eventId });
                }

                TempData["Success"] =
                    "You have successfully enrolled in this event.";

                return RedirectToAction(nameof(MyEnrollments));
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "The API connection failed.";

                return RedirectToAction(
                    "Enroll",
                    new { id = eventId });
            }
        }

        // GET: Participant/MyEnrollments
        [HttpGet]
        public async Task<IActionResult> MyEnrollments()
        {
            var access = CheckAccess();

            if (access != null)
                return access;

            try
            {
                var enrollments =
                    await _api.GetJsonAsync<List<RaceDayMvc.Models.EnrollmentViewModel>>(
                        "api/Enrollments/my");

                if (enrollments != null && enrollments.Any())
                {
                    ViewBag.IsDemo = false;
                    return View(enrollments);
                }
            }
            catch (HttpRequestException)
            {
                // Use labelled demonstration data if the API is unavailable.
            }

            ViewBag.IsDemo = true;

            var demoEnrollments =
                new List<RaceDayMvc.Models.EnrollmentViewModel>
                {
                    new RaceDayMvc.Models.EnrollmentViewModel
                    {
                        EnrollmentID = 1001,
                        EventID = 1,
                        ParticipantID = 1,
                        CategoryID = 1,
                        EnrollmentDate = DateTime.Today.AddDays(-10),
                        RaceNumber = "102",
                        Status = "Registered"
                    },

                    new RaceDayMvc.Models.EnrollmentViewModel
                    {
                        EnrollmentID = 1002,
                        EventID = 2,
                        ParticipantID = 1,
                        CategoryID = 2,
                        EnrollmentDate = DateTime.Today.AddDays(-5),
                        RaceNumber = "101",
                        Status = "Confirmed"
                    }
                };

            return View(demoEnrollments);
        }

        // GET: Participant/MyResults
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            var access = CheckAccess();

            if (access != null)
                return access;

            try
            {
                var enrollments =
                    await _api.GetJsonAsync<List<RaceDayMvc.Models.EnrollmentViewModel>>(
                        "api/Enrollments/my");

                if (enrollments != null)
                {
                    var results =
                        enrollments
                            .Where(e => e.Result != null)
                            .Select(e => new RaceDayMvc.Models.ResultViewModel
                            {
                                ResultID = e.Result!.ResultID,
                                EnrollmentID = e.Result.EnrollmentID,
                                FinishTime = e.Result.FinishTime,
                                Position = e.Result.Position,
                                AveragePace = e.Result.AveragePace,
                                ResultStatus = e.Result.ResultStatus
                            })
                            .ToList();

                    if (results.Any())
                    {
                        ViewBag.IsDemo = false;
                        return View(results);
                    }
                }
            }
            catch (HttpRequestException)
            {
                // Use labelled demonstration data if the API is unavailable.
            }

            ViewBag.IsDemo = true;

            var demoResults =
                new List<RaceDayMvc.Models.ResultViewModel>
                {
                    new RaceDayMvc.Models.ResultViewModel
                    {
                        ResultID = 501,
                        EnrollmentID = 1001,
                        FinishTime = TimeSpan.FromMinutes(48.5),
                        Position = 12,
                        AveragePace = "4:51 min/km",
                        ResultStatus = "Completed"
                    },

                    new RaceDayMvc.Models.ResultViewModel
                    {
                        ResultID = 502,
                        EnrollmentID = 1002,
                        FinishTime = TimeSpan.FromMinutes(65),
                        Position = 24,
                        AveragePace = "6:30 min/km",
                        ResultStatus = "Completed"
                    }
                };

            return View(demoResults);
        }
    }
}


