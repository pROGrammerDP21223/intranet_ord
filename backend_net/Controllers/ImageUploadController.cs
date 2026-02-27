using backend_net.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class ImageUploadController : BaseController
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public ImageUploadController(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string? folder = "products")
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return HandleError("No file uploaded", 400);
            }

            // Validate file size
            if (file.Length > MaxFileSize)
            {
                return HandleError($"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB", 400);
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return HandleError($"File type not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}", 400);
            }

            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", folder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return URL
            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:8080";
            var imageUrl = $"{baseUrl}/uploads/{folder}/{fileName}";

            return HandleSuccess("Image uploaded successfully", new { url = imageUrl, fileName });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred while uploading image: {ex.Message}", 500);
        }
    }

    [HttpPost("upload-multiple")]
    public async Task<IActionResult> UploadMultipleImages([FromForm] List<IFormFile> files, [FromQuery] string? folder = "products")
    {
        try
        {
            if (files == null || files.Count == 0)
            {
                return HandleError("No files uploaded", 400);
            }

            var uploadedUrls = new List<string>();

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                    continue;

                // Validate file size
                if (file.Length > MaxFileSize)
                {
                    continue; // Skip oversized files
                }

                // Validate file extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    continue; // Skip invalid file types
                }

                // Create uploads directory if it doesn't exist
                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Add URL to list
                var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:8080";
                var imageUrl = $"{baseUrl}/uploads/{folder}/{fileName}";
                uploadedUrls.Add(imageUrl);
            }

            if (uploadedUrls.Count == 0)
            {
                return HandleError("No valid files were uploaded", 400);
            }

            return HandleSuccess("Images uploaded successfully", new { urls = uploadedUrls });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred while uploading images: {ex.Message}", 500);
        }
    }

    [HttpDelete("delete")]
    public IActionResult DeleteImage([FromQuery] string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url))
            {
                return HandleError("Image URL is required", 400);
            }

            // Extract file path from URL
            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:8080";
            if (!url.StartsWith(baseUrl))
            {
                return HandleError("Invalid image URL", 400);
            }

            var relativePath = url.Replace(baseUrl, "").TrimStart('/');
            var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, relativePath);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return HandleSuccess("Image deleted successfully");
            }

            return HandleError("Image not found", 404);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred while deleting image: {ex.Message}", 500);
        }
    }
}

