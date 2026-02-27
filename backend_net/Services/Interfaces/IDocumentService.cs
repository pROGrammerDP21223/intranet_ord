using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;

namespace backend_net.Services.Interfaces;

public interface IDocumentService
{
    System.Threading.Tasks.Task<IEnumerable<Document>> GetDocumentsByEntityAsync(string entityType, int entityId);
    System.Threading.Tasks.Task<Document?> GetDocumentByIdAsync(int id);
    System.Threading.Tasks.Task<Document> UploadDocumentAsync(UploadDocumentRequest request, int? uploadedBy = null);
    System.Threading.Tasks.Task<Document> CreateDocumentRecordAsync(string fileName, string filePath, string fileType, long fileSize, string entityType, int entityId, string? category, string? description, string? tags, int? uploadedBy);
    System.Threading.Tasks.Task<bool> DeleteDocumentAsync(int id, int? userId = null);
    System.Threading.Tasks.Task<Document> CreateNewVersionAsync(int documentId, UploadDocumentRequest request, int? uploadedBy = null);
    System.Threading.Tasks.Task<IEnumerable<Document>> GetDocumentVersionsAsync(int documentId);
}

