
using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    public class BannerController : Controller
    {
        private readonly BlobStorageService _blobStorage;

        public BannerController(BlobStorageService blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public IActionResult Upload()
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Login", "Auth");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Login", "Auth");

            if (image == null || image.Length == 0)
            {
                ModelState.AddModelError(
                    "image", "Please select an image.");

                return View();
            }

            try
            {
                var fileName =
                    await _blobStorage.UploadImageAsync(image);

                ViewBag.ImageUrl =
                    Url.Action("Image", "Banner",
                        new { fileName = fileName });

                ViewBag.Success =
                    "Your event banner was uploaded successfully.";

                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "", "Upload failed: " + ex.Message);

                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Image(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) ||
                fileName != Path.GetFileName(fileName))
            {
                return BadRequest();
            }

            try
            {
                var image = await _blobStorage.GetImageAsync(fileName);

                return File(image.Content, image.ContentType);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
