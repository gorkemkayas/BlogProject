using Microsoft.AspNetCore.Mvc;

namespace BlogProject.Controllers
{
    [Route("upload")]
    [RequestSizeLimit(200 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 200 * 1024 * 1024)]
    public class UploadController : Controller
    {
        [RequestSizeLimit(50_000_000)] // 50 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
                return BadRequest(new { uploaded = false, error = new { message = "No file uploaded." } });

            var fileExtension = Path.GetExtension(upload.FileName);
            var fileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)); // klasör yoksa oluştur

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await upload.CopyToAsync(stream);
            }

            var url = $"/uploads/{fileName}";

            return Ok(new
            {
                uploaded = true,
                url = url
            });
        }
    }
}
