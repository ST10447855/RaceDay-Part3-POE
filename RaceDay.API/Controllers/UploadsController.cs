using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.API.Controllers
{
    /// <summary>Image uploads. Files are stored in Azure Blob Storage; the MVC app never talks to Azure directly.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UploadsController : ControllerBase
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxBytes = 5 * 1024 * 1024;

        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public UploadsController(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        /// <summary>Uploads an event banner or profile picture and returns its public URL.</summary>
        /// <remarks>Multipart form field name: <c>file</c>. Max 5 MB. jpg, png, gif or webp only.</remarks>
        /// <response code="200">Returns { url }</response>
        /// <response code="400">No file, wrong type or too large</response>
        [HttpPost("image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Please choose an image to upload." });

            if (file.Length > MaxBytes)
                return BadRequest(new { message = "The image must be 5 MB or smaller." });

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return BadRequest(new { message = "Only jpg, png, gif or webp images are allowed." });

            var blobName = $"{Guid.NewGuid():N}{extension}";
            var connectionString = _config["AzureStorage:ConnectionString"];

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var containerName = _config["AzureStorage:ContainerName"] ?? "raceday-images";
                var container = new BlobContainerClient(connectionString, containerName);
                await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blob = container.GetBlobClient(blobName);
                await using var stream = file.OpenReadStream();
                await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

                return Ok(new { url = blob.Uri.ToString() });
            }

            // Local fallback so the app still works without an Azure account
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var folder = Path.Combine(webRoot, "uploads");
            Directory.CreateDirectory(folder);
            await using (var fs = new FileStream(Path.Combine(folder, blobName), FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            return Ok(new { url = $"{Request.Scheme}://{Request.Host}/uploads/{blobName}" });
        }
    }
}
