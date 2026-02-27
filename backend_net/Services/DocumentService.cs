using backend_net.Data.Context;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend_net.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DocumentService> _logger;
    private readonly string _uploadsPath;

    public DocumentService(ApplicationDbContext context, ILogger<DocumentService> logger, IWebHostEnvironment environment)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _uploadsPath = Path.Combine(environment.WebRootPath ?? environment.ContentRootPath, "uploads", "documents");
        
        if (!Directory.Exists(_uploadsPath))
        {
            Directory.CreateDirectory(_uploadsPath);
        }
    }

    public async System.Threading.Tasks.Task<IEnumerable<Document>> GetDocumentsByEntityAsync(string entityType, int entityId)
    {
        return await _context.Documents
            .Where(d => !d.IsDeleted && d.EntityType == entityType && d.EntityId == entityId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<Document?> GetDocumentByIdAsync(int id)
    {
        return await _context.Documents
            .Include(d => d.ParentDocument)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
    }

    public async System.Threading.Tasks.Task<Document> UploadDocumentAsync(UploadDocumentRequest request, int? uploadedBy = null)
    {
        // This method should be called from controller after file is saved
        // For now, we'll create a placeholder - actual file upload handling is in controller
        throw new NotImplementedException("Use controller's file upload endpoint");
    }

    public async System.Threading.Tasks.Task<Document> CreateDocumentRecordAsync(
        string fileName,
        string filePath,
        string fileType,
        long fileSize,
        string entityType,
        int entityId,
        string? category,
        string? description,
        string? tags,
        int? uploadedBy)
    {
        var document = new Document
        {
            FileName = fileName,
            FilePath = filePath,
            FileType = fileType,
            FileSize = fileSize,
            EntityType = entityType,
            EntityId = entityId,
            Category = category,
            Description = description,
            Tags = tags,
            UploadedBy = uploadedBy,
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        if (uploadedBy.HasValue)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == uploadedBy.Value && !u.IsDeleted);
            if (user != null)
            {
                document.UploadedByName = user.Name;
            }
        }

        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();

        return document;
    }

    public async System.Threading.Tasks.Task<bool> DeleteDocumentAsync(int id, int? userId = null)
    {
        var document = await GetDocumentByIdAsync(id);
        if (document == null)
        {
            return false;
        }

        // Soft delete
        document.IsDeleted = true;
        document.UpdatedAt = DateTime.UtcNow;

        // Optionally delete physical file
        try
        {
            if (File.Exists(document.FilePath))
            {
                File.Delete(document.FilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete physical file for document {DocumentId}", id);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async System.Threading.Tasks.Task<Document> CreateNewVersionAsync(int documentId, UploadDocumentRequest request, int? uploadedBy = null)
    {
        var originalDocument = await GetDocumentByIdAsync(documentId);
        if (originalDocument == null)
        {
            throw new KeyNotFoundException($"Document with ID {documentId} not found");
        }

        // Get latest version
        var latestVersion = await _context.Documents
            .Where(d => (d.Id == documentId || d.ParentDocumentId == documentId) && !d.IsDeleted)
            .OrderByDescending(d => d.Version)
            .FirstOrDefaultAsync();

        var newVersion = latestVersion?.Version + 1 ?? 2;
        var parentId = originalDocument.ParentDocumentId ?? originalDocument.Id;

        // This should be called from controller after file is saved
        throw new NotImplementedException("Use controller's file upload endpoint with version parameter");
    }

    public async System.Threading.Tasks.Task<IEnumerable<Document>> GetDocumentVersionsAsync(int documentId)
    {
        var document = await GetDocumentByIdAsync(documentId);
        if (document == null)
        {
            return new List<Document>();
        }

        var parentId = document.ParentDocumentId ?? document.Id;

        return await _context.Documents
            .Where(d => (d.Id == parentId || d.ParentDocumentId == parentId) && !d.IsDeleted)
            .OrderByDescending(d => d.Version)
            .ToListAsync();
    }
}

