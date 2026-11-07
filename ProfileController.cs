
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDayMvc.Models;

namespace RaceDayMvc.Controllers
{
    public class ProfileController : Controller
    {
        // GET: Profile
        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserID");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var fullName =
                HttpContext.Session.GetString("FullName") ?? "";

            var names = fullName.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

            var model = new ProfileForm
            {
                FirstName = names.FirstOrDefault() ?? "",
                LastName = string.Join(" ", names.Skip(1)),
                Email = HttpContext.Session.GetString("Email") ?? ""
            };

            return View(model);
        }

        // POST: Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ProfileForm model)
        {
            var userId = HttpContext.Session.GetString("UserID");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ModelState.AddModelError(
                "",
                "Profile editing is not available yet because the API does not currently have a profile-update endpoint."
            );

            return View(model);
        }
    }
}


