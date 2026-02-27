using backend_net.Data.Context;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DocumentsController : BaseController
{
    private readonly IDocumentService _documentService;
    private readonly ApplicationDbContext _context;
    private readonly IAccessControlService _accessControlService;
    private readonly IWebHostEnvironment _environment;
    private readonly string _uploadsPath;

    public DocumentsController(
        IDocumentService documentService,
        ApplicationDbContext context,
        IAccessControlService accessControlService,
        IWebHostEnvironment environment)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _accessControlService = accessControlService ?? throw new ArgumentNullException(nameof(accessControlService));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _uploadsPath = Path.Combine(environment.WebRootPath ?? environment.ContentRootPath, "uploads", "documents");
        
        if (!Directory.Exists(_uploadsPath))
        {
            Directory.CreateDirectory(_uploadsPath);
        }
    }

    /// <summary>
    /// Get documents for an entity
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<IActionResult> GetDocumentsByEntity(string entityType, int entityId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Check access based on entity type
            if (entityType == "Client")
            {
                if (!await _accessControlService.CanUserAccessClientAsync(userId.Value, entityId))
                {
                    return HandleError("Unauthorized: You don't have access to this client's documents", 403);
                }
            }

            var documents = await _documentService.GetDocumentsByEntityAsync(entityType, entityId);
            return HandleSuccess("Documents retrieved successfully", documents);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocumentById(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return HandleError("Document not found", 404);
            }

            // Check access based on entity type
            if (document.EntityType == "Client")
            {
                if (!await _accessControlService.CanUserAccessClientAsync(userId.Value, document.EntityId))
                {
                    return HandleError("Unauthorized: You don't have access to this document", 403);
                }
            }

            return HandleSuccess("Document retrieved successfully", document);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Upload a document
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument([FromForm] IFormFile file, [FromForm] UploadDocumentRequest request)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return HandleError("No file uploaded", 400);
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Check access based on entity type
            if (request.EntityType == "Client")
            {
                if (!await _accessControlService.CanUserAccessClientAsync(userId.Value, request.EntityId))
                {
                    return HandleError("Unauthorized: You don't have access to upload documents for this client", 403);
                }
            }

            // Validate file size (50MB max)
            const long maxFileSize = 50 * 1024 * 1024; // 50MB
            if (file.Length > maxFileSize)
            {
                return HandleError("File size exceeds maximum allowed size of 50MB", 400);
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadsPath, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create document record
            var document = await _documentService.CreateDocumentRecordAsync(
                file.FileName,
                filePath,
                fileExtension.TrimStart('.'),
                file.Length,
                request.EntityType,
                request.EntityId,
                request.Category,
                request.Description,
                request.Tags,
                userId.Value);

            return StatusCode(201, new { message = "Document uploaded successfully", data = document, success = true });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Download a document
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return HandleError("Document not found", 404);
            }

            // Check access
            if (document.EntityType == "Client")
            {
                if (!await _accessControlService.CanUserAccessClientAsync(userId.Value, document.EntityId))
                {
                    return HandleError("Unauthorized: You don't have access to this document", 403);
                }
            }

            if (!System.IO.File.Exists(document.FilePath))
            {
                return HandleError("File not found on server", 404);
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(document.FilePath);
            return File(fileBytes, GetContentType(document.FileType), document.FileName);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Delete a document
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return HandleError("Document not found", 404);
            }

            // Check access
            if (document.EntityType == "Client")
            {
                if (!await _accessControlService.CanUserAccessClientAsync(userId.Value, document.EntityId))
                {
                    return HandleError("Unauthorized: You don't have access to delete this document", 403);
                }
            }

            var result = await _documentService.DeleteDocumentAsync(id, userId.Value);
            if (!result)
            {
                return HandleError("Failed to delete document", 500);
            }

            return HandleSuccess("Document deleted successfully", null);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get document versions
    /// </summary>
    [HttpGet("{id}/versions")]
    public async Task<IActionResult> GetDocumentVersions(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return HandleError("Document not found", 404);
            }

            // Check access
            if (document.EntityType == "Client")
            {
                if (!await _accessControlService.CanUserAccessClientAsync(userId.Value, document.EntityId))
                {
                    return HandleError("Unauthorized: You don't have access to this document", 403);
                }
            }

            var versions = await _documentService.GetDocumentVersionsAsync(id);
            return HandleSuccess("Document versions retrieved successfully", versions);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    private string GetContentType(string fileType)
    {
        return fileType.ToLower() switch
        {
            "pdf" => "application/pdf",
            "doc" => "application/msword",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xls" => "application/vnd.ms-excel",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}

