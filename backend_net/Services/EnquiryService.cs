using backend_net.Data.Context;
using backend_net.Data.Repositories;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using backend_net.Services.Notifications.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace backend_net.Services;

/// <summary>
/// Enquiry Service - Refactored to follow SOLID principles
/// - Single Responsibility: Handles enquiry business logic only
/// - Dependency Inversion: Depends on abstractions (IEnquiryRepository, INotificationService)
/// - Open/Closed: Can be extended without modification
/// </summary>
public class EnquiryService : IEnquiryService
{
    private readonly IEnquiryRepository _repository;
    private readonly ApplicationDbContext _context; // For client lookup
    private readonly INotificationService _notificationService;
    private readonly ISignalRNotificationService? _signalRNotificationService;
    private readonly ILogger<EnquiryService> _logger;

    public EnquiryService(
        IEnquiryRepository repository,
        ApplicationDbContext context,
        INotificationService notificationService,
        ISignalRNotificationService? signalRNotificationService,
        ILogger<EnquiryService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _signalRNotificationService = signalRNotificationService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async System.Threading.Tasks.Task<Enquiry> CreateAsync(CreateEnquiryRequest request)
    {
        // Create enquiry entity
        var enquiry = CreateEnquiryFromRequest(request);

        // Save enquiry using repository
        await _repository.AddAsync(enquiry);
        await _context.SaveChangesAsync();

        // Send notifications asynchronously (fire and forget)
        // Notifications are not critical for enquiry creation
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                var client = await _context.Clients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == enquiry.ClientId && !c.IsDeleted);

                if (client != null)
                {
                    try
                    {
                        var result = await _notificationService.SendEnquiryNotificationsAsync(enquiry, client);
                        if (result.IsFailure)
                        {
                            _logger.LogWarning("Failed to send notifications for enquiry {EnquiryId}: {Error}",
                                enquiry.Id, result.ErrorMessage);
                        }

                        // Send SignalR notification
                        if (_signalRNotificationService != null)
                        {
                            await _signalRNotificationService.NotifyNewEnquiryAsync(enquiry, client);
                        }
                    }
                    catch (Exception notificationEx)
                    {
                        _logger.LogError(notificationEx, "Exception in notification service for enquiry {EnquiryId}", enquiry.Id);
                    }
                }
                else
                {
                    _logger.LogWarning("Client {ClientId} not found for enquiry {EnquiryId} notifications", enquiry.ClientId, enquiry.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notifications for enquiry {EnquiryId}", enquiry.Id);
            }
        });

        return enquiry;
    }

    /// <summary>
    /// Factory method to create Enquiry from request
    /// Encapsulates entity creation logic
    /// </summary>
    private static Enquiry CreateEnquiryFromRequest(CreateEnquiryRequest request)
    {
        string? rawPayloadJson = null;
        if (request.RawPayload.HasValue)
        {
            rawPayloadJson = request.RawPayload.Value.GetRawText();
        }

        return new Enquiry
        {
            FullName = request.FullName.Trim(),
            MobileNumber = request.MobileNumber.Trim(),
            EmailId = request.EmailId.Trim().ToLowerInvariant(),
            ClientId = request.ClientId,
            Status = "New",
            Source = request.Source?.Trim() ?? "Website",
            ReferrerUrl = request.ReferrerUrl?.Trim(),
            RawPayload = rawPayloadJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public async System.Threading.Tasks.Task<Enquiry?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetAllAsync(DateTime? startDate, DateTime? endDate)
    {
        return await _repository.GetAllAsync(startDate, endDate);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetFilteredByClientIdsAsync(List<int> clientIds)
    {
        return await _repository.GetFilteredByClientIdsAsync(clientIds);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetFilteredByClientIdsAsync(List<int> clientIds, DateTime? startDate, DateTime? endDate)
    {
        return await _repository.GetFilteredByClientIdsAsync(clientIds, startDate, endDate);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByStatusAsync(string status)
    {
        return await _repository.GetByStatusAsync(status);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByStatusAsync(string status, DateTime? startDate, DateTime? endDate)
    {
        return await _repository.GetByStatusAsync(status, startDate, endDate);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByStatusFilteredByClientIdsAsync(string status, List<int> clientIds)
    {
        return await _repository.GetByStatusFilteredByClientIdsAsync(status, clientIds);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByStatusFilteredByClientIdsAsync(string status, List<int> clientIds, DateTime? startDate, DateTime? endDate)
    {
        return await _repository.GetByStatusFilteredByClientIdsAsync(status, clientIds, startDate, endDate);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByClientIdAsync(int clientId)
    {
        return await _repository.GetByClientIdAsync(clientId);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByClientIdAsync(int clientId, DateTime? startDate, DateTime? endDate)
    {
        return await _repository.GetByClientIdAsync(clientId, startDate, endDate);
    }

    public async System.Threading.Tasks.Task<Enquiry> UpdateAsync(int id, UpdateEnquiryRequest request)
    {
        var enquiry = await _repository.GetByIdAsync(id);
        if (enquiry == null || enquiry.IsDeleted)
        {
            throw new KeyNotFoundException("Enquiry not found");
        }

        // Update enquiry using domain logic
        UpdateEnquiryFromRequest(enquiry, request);

        await _repository.UpdateAsync(enquiry);
        await _context.SaveChangesAsync();

        return enquiry;
    }

    /// <summary>
    /// Updates enquiry entity from request
    /// Encapsulates update logic
    /// </summary>
    private static void UpdateEnquiryFromRequest(Enquiry enquiry, UpdateEnquiryRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            enquiry.Status = request.Status.Trim();
            if (request.Status == "Resolved" || request.Status == "Closed")
            {
                enquiry.ResolvedAt = DateTime.UtcNow;
            }
        }

        if (request.Notes != null)
        {
            enquiry.Notes = request.Notes.Trim();
        }

        enquiry.UpdatedAt = DateTime.UtcNow;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
    {
        var enquiry = await _repository.GetByIdAsync(id);
        if (enquiry == null || enquiry.IsDeleted)
        {
            return false;
        }

        await _repository.DeleteAsync(enquiry);
        await _context.SaveChangesAsync();

        return true;
    }

    public async System.Threading.Tasks.Task<int> GetCountByStatusAsync(string status)
    {
        return await _repository.GetCountByStatusAsync(status);
    }

    public async System.Threading.Tasks.Task<int> GetCountByStatusFilteredByClientIdsAsync(string status, List<int> clientIds)
    {
        return await _repository.GetCountByStatusFilteredByClientIdsAsync(status, clientIds);
    }

    public async System.Threading.Tasks.Task<int> GetTotalCountAsync()
    {
        return await _repository.GetTotalCountAsync();
    }

    public async System.Threading.Tasks.Task<int> GetTotalCountFilteredByClientIdsAsync(List<int> clientIds)
    {
        return await _repository.GetTotalCountFilteredByClientIdsAsync(clientIds);
    }
}

