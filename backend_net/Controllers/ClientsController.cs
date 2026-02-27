using backend_net.Controllers;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend_net.Data.Context;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ClientsController : BaseController
{
    private readonly IClientService _clientService;
    private readonly IAccessControlService _accessControlService;
    private readonly ApplicationDbContext _context;

    public ClientsController(IClientService clientService, IAccessControlService accessControlService, ApplicationDbContext context)
    {
        _clientService = clientService;
        _accessControlService = accessControlService;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
    {
        try
        {
            // Allow Admin, Owner, Sales Person, and Sales Manager to create clients
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var userRole = GetCurrentUserRole();
            var canCreate = IsAdmin() || IsSalesPerson() || IsSalesManager();
            
            if (!canCreate)
            {
                return HandleError("Unauthorized: Only Admin, Owner, Sales Person, and Sales Manager can create clients", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdByRole = GetCurrentUserRole();
            var client = await _clientService.CreateAsync(request, userId.Value, createdByRole);
            
            // Project to DTO to avoid circular references
            var clientDto = MapToDto(client);
            
            return HandleSuccess("Client created successfully", clientDto);
        }
        catch (InvalidOperationException ex)
        {
            return HandleError(ex.Message, 400);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetClients()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            List<Client> clients;

            // Admin/Owner/HOD/Calling Staff/Employee can see all clients
            if (IsAdmin() || IsHOD() || IsCallingStaff() || IsEmployee())
            {
                clients = (await _clientService.GetAllAsync()).ToList();
            }
            else
            {
                // Get accessible client IDs based on role and relationships
                var accessibleClientIds = await _accessControlService.GetAccessibleClientIdsAsync(userId.Value);
                clients = (await _clientService.GetAllAsync())
                    .Where(c => accessibleClientIds.Contains(c.Id))
                    .ToList();
            }

            var clientsDto = clients.Select(MapToDto).ToList();
            return HandleSuccess("Clients retrieved successfully", clientsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClientById(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Check if user can access this client
            if (!IsAdmin() && !IsHOD() && !IsCallingStaff() && !IsEmployee() && !await _accessControlService.CanUserAccessClientAsync(userId.Value, id))
            {
                return HandleError("Unauthorized: You don't have access to this client", 403);
            }

            var client = await _clientService.GetByIdAsync(id);
            
            if (client == null)
            {
                return HandleError("Client not found", 404);
            }

            var clientDto = MapToDto(client);
            return HandleSuccess("Client retrieved successfully", clientDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClient(int id, [FromBody] CreateClientRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Only Admin/Owner/Sales Person/Sales Manager can edit clients (Clients can only view)
            // But only Admin/Owner/Sales Person/Sales Manager can manage WhatsApp/Email settings
            if (!IsAdmin() && !IsSalesPerson() && !IsSalesManager())
            {
                return HandleError("Unauthorized: Only Admin, Owner, Sales Person, and Sales Manager can edit clients", 403);
            }

            // Check if user can access this client (for Sales Person/Sales Manager)
            if (!IsAdmin() && !await _accessControlService.CanUserAccessClientAsync(userId.Value, id))
            {
                return HandleError("Unauthorized: You don't have access to this client", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = await _clientService.UpdateAsync(id, request);
            var clientDto = MapToDto(client);
            
            return HandleSuccess("Client updated successfully", clientDto);
        }
        catch (KeyNotFoundException ex)
        {
            return HandleError(ex.Message, 404);
        }
        catch (InvalidOperationException ex)
        {
            return HandleError(ex.Message, 400);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        try
        {
            // Only Admin/Owner can delete clients
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can delete clients", 403);
            }

            await _clientService.DeleteAsync(id);
            return HandleSuccess("Client deleted successfully");
        }
        catch (KeyNotFoundException ex)
        {
            return HandleError(ex.Message, 404);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    // Helper method to map Entity to DTO (avoids circular references)
    private object MapToDto(Client client)
    {
        var clientServices = client.ClientServices != null && client.ClientServices.Any()
            ? client.ClientServices.Select(cs => new
            {
                cs.Id,
                cs.ServiceId,
                Service = cs.Service != null ? new
                {
                    cs.Service.Id,
                    cs.Service.ServiceType,
                    cs.Service.ServiceName,
                    cs.Service.Category
                } : (object?)null
            }).Cast<object>().ToList()
            : new List<object>();

        var clientEmailServices = client.ClientEmailServices != null && client.ClientEmailServices.Any()
            ? client.ClientEmailServices.Select(es => new
            {
                es.Id,
                es.EmailServiceType,
                es.Quantity
            }).Cast<object>().ToList()
            : new List<object>();

        object? clientSeoDetail = client.ClientSeoDetail != null ? new
        {
            client.ClientSeoDetail.Id,
            client.ClientSeoDetail.KeywordRange,
            client.ClientSeoDetail.Location,
            client.ClientSeoDetail.KeywordsList
        } : null;

        object? clientAdwordsDetail = client.ClientAdwordsDetail != null ? new
        {
            client.ClientAdwordsDetail.Id,
            client.ClientAdwordsDetail.NumberOfKeywords,
            client.ClientAdwordsDetail.Period,
            client.ClientAdwordsDetail.Location,
            client.ClientAdwordsDetail.KeywordsList,
            client.ClientAdwordsDetail.SpecialGuidelines
        } : null;

        return new
        {
            client.Id,
            client.CustomerNo,
            client.FormDate,
            client.AmountWithoutGst,
            client.GstPercentage,
            client.GstAmount,
            client.TotalPackage,
            client.CompanyName,
            client.ContactPerson,
            client.Designation,
            client.Address,
            client.Phone,
            client.Email,
            client.WhatsAppNumber,
            client.EnquiryEmail,
            client.UseWhatsAppService,
            client.WhatsAppSameAsMobile,
            client.UseSameEmailForEnquiries,
            client.DomainName,
            client.GstNo,
            client.SpecificGuidelines,
            client.CreatedAt,
            client.UpdatedAt,
            client.CreatedBy,
            client.CreatedByUserId,
            client.Status,
            client.AssignedToSalesPersonName,
            ClientServices = clientServices,
            ClientEmailServices = clientEmailServices,
            ClientSeoDetail = clientSeoDetail,
            ClientAdwordsDetail = clientAdwordsDetail
        };
    }

    /// <summary>
    /// Approve a pending client - Only Admin/Owner/Calling Staff can approve
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveClient(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Only Admin/Owner/Calling Staff can approve clients
            if (!IsAdmin() && !IsCallingStaff())
            {
                return HandleError("Unauthorized: Only Admin, Owner, and Calling Staff can approve clients", 403);
            }

            var client = await _clientService.ApproveClientAsync(id, userId.Value);
            var clientDto = MapToDto(client);

            return HandleSuccess("Client approved successfully", clientDto);
        }
        catch (KeyNotFoundException ex)
        {
            return HandleError(ex.Message, 404);
        }
        catch (UnauthorizedAccessException ex)
        {
            return HandleError(ex.Message, 403);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

