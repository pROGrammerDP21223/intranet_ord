using backend_net.Data.Context;
using backend_net.Data.Interfaces;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace backend_net.Services;

public class ClientService : IClientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly ISignalRNotificationService? _signalRNotificationService;
    private readonly IWorkflowService? _workflowService;
    private readonly IEmailService? _emailService;
    private readonly IConfiguration? _configuration;

    public ClientService(
        IUnitOfWork unitOfWork, 
        ApplicationDbContext context, 
        ISignalRNotificationService? signalRNotificationService = null,
        IWorkflowService? workflowService = null,
        IEmailService? emailService = null,
        IConfiguration? configuration = null)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _signalRNotificationService = signalRNotificationService;
        _workflowService = workflowService;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async System.Threading.Tasks.Task<Client?> GetByIdAsync(int id)
    {
        return await _context.Clients
            .Include(c => c.ClientServices!)
                .ThenInclude(cs => cs.Service)
            .Include(c => c.ClientEmailServices!)
            .Include(c => c.ClientSeoDetail)
            .Include(c => c.ClientAdwordsDetail)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Client>> GetAllAsync()
    {
        return await _context.Clients
            .Where(c => !c.IsDeleted)
            .Include(c => c.ClientServices!)
                .ThenInclude(cs => cs.Service)
            .Include(c => c.ClientEmailServices!)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    private async System.Threading.Tasks.Task<string> GenerateCustomerNoAsync()
    {
        var today = DateTime.Now;
        var prefix = $"ORD-{today:ddMMyyyy}";

        var existingNos = await _context.Clients
            .Where(c => c.CustomerNo.StartsWith(prefix))
            .Select(c => c.CustomerNo)
            .ToListAsync();

        int maxSeq = 0;
        foreach (var no in existingNos)
        {
            var parts = no.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int seq) && seq > maxSeq)
                maxSeq = seq;
        }

        return $"{prefix}-{(maxSeq + 1):D3}";
    }

    public async System.Threading.Tasks.Task<Client> CreateAsync(CreateClientRequest request, int? createdByUserId = null, string? createdByRole = null)
    {
        // Auto-generate customer number if not provided
        if (string.IsNullOrWhiteSpace(request.CustomerNo))
        {
            request.CustomerNo = await GenerateCustomerNoAsync();
        }

        // Check if customer number already exists
        if (await CustomerNoExistsAsync(request.CustomerNo))
        {
            throw new InvalidOperationException("Client with this customer number already exists");
        }

        string createdBy = "System";
        if (createdByUserId.HasValue)
        {
            var creator = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == createdByUserId.Value && !u.IsDeleted);
            if (creator != null)
                createdBy = creator.Name;
        }

        var client = new Client
        {
            CustomerNo = request.CustomerNo,
            FormDate = request.FormDate,
            AmountWithoutGst = request.AmountWithoutGst,
            GstPercentage = request.GstPercentage,
            GstAmount = request.GstAmount,
            TotalPackage = request.TotalPackage,
            CompanyName = request.CompanyName,
            ContactPerson = request.ContactPerson,
            Designation = request.Designation,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            WhatsAppNumber = request.WhatsAppNumber,
            EnquiryEmail = request.EnquiryEmail,
            UseWhatsAppService = request.UseWhatsAppService,
            WhatsAppSameAsMobile = request.WhatsAppSameAsMobile,
            UseSameEmailForEnquiries = request.UseSameEmailForEnquiries,
            DomainName = request.DomainName,
            CompanyLogo = request.CompanyLogo,
            GstNo = request.GstNo,
            SpecificGuidelines = request.SpecificGuidelines,
            CreatedBy = createdBy,
            CreatedByUserId = createdByUserId,
            Status = "Approved"
        };

        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();

        // Auto-attach creator to client based on role (Admin/Owner can change via Manage User Relationships)
        if (createdByUserId.HasValue)
        {
            var creator = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == createdByUserId.Value && !u.IsDeleted);
            // Use DB role, fallback to role from JWT (passed by controller)
            var roleName = (creator?.Role?.Name ?? createdByRole ?? "").Trim();

            if (roleName.Equals("Sales Person", StringComparison.OrdinalIgnoreCase))
            {
                await _context.SalesPersonClients.AddAsync(new SalesPersonClient
                {
                    SalesPersonId = createdByUserId.Value,
                    ClientId = client.Id
                });
                client.AssignedToSalesPersonName = creator?.Name;
            }
            else if (roleName.Equals("Sales Manager", StringComparison.OrdinalIgnoreCase) || roleName.Equals("SalesManager", StringComparison.OrdinalIgnoreCase))
            {
                await _context.SalesManagerClients.AddAsync(new SalesManagerClient
                {
                    SalesManagerId = createdByUserId.Value,
                    ClientId = client.Id
                });
            }
            else if (roleName.Equals("Owner", StringComparison.OrdinalIgnoreCase))
            {
                await _context.OwnerClients.AddAsync(new OwnerClient
                {
                    OwnerId = createdByUserId.Value,
                    ClientId = client.Id
                });
            }
            await _context.SaveChangesAsync();
        }

        // Add services
        if (request.Services != null && request.Services.Any())
        {
            var clientServices = request.Services.Select(s => new Domain.Entities.ClientService
            {
                ClientId = client.Id,
                ServiceId = s.ServiceId
            }).ToList();

            await _context.ClientServices.AddRangeAsync(clientServices);
        }

        // Add email services
        if (request.EmailServices != null && request.EmailServices.Any())
        {
            var clientEmailServices = request.EmailServices.Select(es => new ClientEmailService
            {
                ClientId = client.Id,
                EmailServiceType = es.EmailServiceType,
                Quantity = es.Quantity
            }).ToList();

            await _context.ClientEmailServices.AddRangeAsync(clientEmailServices);
        }

        // Add SEO details
        if (request.SeoDetail != null)
        {
            var seoDetail = new ClientSeoDetail
            {
                ClientId = client.Id,
                KeywordRange = request.SeoDetail.KeywordRange,
                Location = request.SeoDetail.Location,
                KeywordsList = request.SeoDetail.KeywordsList
            };

            await _context.ClientSeoDetails.AddAsync(seoDetail);
        }

        // Add AdWords details
        if (request.AdwordsDetail != null)
        {
            var adwordsDetail = new ClientAdwordsDetail
            {
                ClientId = client.Id,
                NumberOfKeywords = request.AdwordsDetail.NumberOfKeywords,
                Period = request.AdwordsDetail.Period,
                Location = request.AdwordsDetail.Location,
                KeywordsList = request.AdwordsDetail.KeywordsList,
                SpecialGuidelines = request.AdwordsDetail.SpecialGuidelines
            };

            await _context.ClientAdwordsDetails.AddAsync(adwordsDetail);
        }

        await _context.SaveChangesAsync();

        // Trigger workflows
        if (_workflowService != null)
        {
            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    await _workflowService.ExecuteWorkflowsAsync("Client", "Created", client.Id, client);
                }
                catch (Exception ex)
                {
                    // Log but don't fail client creation
                }
            });
        }

        // Send email notifications (to client, creator, and owner)
        if (_emailService != null)
        {
            var creator = createdByUserId.HasValue
                ? await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == createdByUserId.Value && !u.IsDeleted)
                : null;
            var clientCompanyName = request.CompanyName ?? "Unknown";
            var createdByName = creator?.Name ?? createdBy ?? "System";

            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    // 1. Send to the client (company whose form was filled)
                    if (!string.IsNullOrWhiteSpace(request.Email))
                    {
                        var clientRecipientName = !string.IsNullOrWhiteSpace(request.ContactPerson) ? request.ContactPerson : clientCompanyName;
                        await _emailService.SendClientCreatedNotificationAsync(
                            request.Email, clientRecipientName, clientCompanyName, createdByName, "client");
                    }

                    // 2. Send to the creator (user who filled the form)
                    if (creator != null && !string.IsNullOrWhiteSpace(creator.Email))
                    {
                        await _emailService.SendClientCreatedNotificationAsync(
                            creator.Email, creator.Name, clientCompanyName, createdByName, "creator");
                    }

                    // 3. Send to owner(s) - users with Owner role
                    var ownerEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var owners = await _context.Users
                        .Include(u => u.Role)
                        .Where(u => !u.IsDeleted && u.IsActive && u.Role != null && u.Role.Name == "Owner")
                        .ToListAsync();
                    foreach (var owner in owners)
                    {
                        if (!string.IsNullOrWhiteSpace(owner.Email))
                            ownerEmails.Add(owner.Email);
                    }

                    // Also include AdminEmail from config if set
                    var adminEmail = _configuration?["Email:AdminEmail"] ?? _configuration?["Email:FromEmail"] ?? "";
                    if (!string.IsNullOrWhiteSpace(adminEmail))
                        ownerEmails.Add(adminEmail);

                    foreach (var ownerEmail in ownerEmails)
                    {
                        var ownerUser = owners.FirstOrDefault(o => string.Equals(o.Email, ownerEmail, StringComparison.OrdinalIgnoreCase));
                        var ownerName = ownerUser?.Name ?? "Owner";
                        await _emailService.SendClientCreatedNotificationAsync(
                            ownerEmail, ownerName, clientCompanyName, createdByName, "owner");
                    }
                }
                catch
                {
                    // Log but don't fail client creation - emails are best-effort
                }
            });
        }

        // Return created client with related data
        return await GetByIdAsync(client.Id) ?? client;
    }

    public async System.Threading.Tasks.Task<Client> UpdateAsync(int id, CreateClientRequest request)
    {
        var client = await GetByIdAsync(id);
        if (client == null)
        {
            throw new KeyNotFoundException("Client not found");
        }

        // Check if customer number is being changed and already exists
        if (request.CustomerNo != client.CustomerNo && await CustomerNoExistsAsync(request.CustomerNo, id))
        {
            throw new InvalidOperationException("Client with this customer number already exists");
        }

        // Update main client entity
        client.CustomerNo = request.CustomerNo;
        client.FormDate = request.FormDate;
        client.AmountWithoutGst = request.AmountWithoutGst;
        client.GstPercentage = request.GstPercentage;
        client.GstAmount = request.GstAmount;
        client.TotalPackage = request.TotalPackage;
        client.CompanyName = request.CompanyName;
        client.ContactPerson = request.ContactPerson;
        client.Designation = request.Designation;
        client.Address = request.Address;
        client.Phone = request.Phone;
        client.Email = request.Email;
        client.WhatsAppNumber = request.WhatsAppNumber;
        client.EnquiryEmail = request.EnquiryEmail;
        client.UseWhatsAppService = request.UseWhatsAppService;
        client.WhatsAppSameAsMobile = request.WhatsAppSameAsMobile;
        client.UseSameEmailForEnquiries = request.UseSameEmailForEnquiries;
        client.DomainName = request.DomainName;
        client.CompanyLogo = request.CompanyLogo;
        client.GstNo = request.GstNo;
        client.SpecificGuidelines = request.SpecificGuidelines;
        client.UpdatedAt = DateTime.UtcNow;

        // Update services - remove old and add new
        if (client.ClientServices != null)
        {
            _context.ClientServices.RemoveRange(client.ClientServices);
        }

        if (request.Services != null && request.Services.Any())
        {
            var clientServices = request.Services.Select(s => new Domain.Entities.ClientService
            {
                ClientId = client.Id,
                ServiceId = s.ServiceId
            }).ToList();

            await _context.ClientServices.AddRangeAsync(clientServices);
        }

        // Update email services
        if (client.ClientEmailServices != null)
        {
            _context.ClientEmailServices.RemoveRange(client.ClientEmailServices);
        }

        if (request.EmailServices != null && request.EmailServices.Any())
        {
            var clientEmailServices = request.EmailServices.Select(es => new ClientEmailService
            {
                ClientId = client.Id,
                EmailServiceType = es.EmailServiceType,
                Quantity = es.Quantity
            }).ToList();

            await _context.ClientEmailServices.AddRangeAsync(clientEmailServices);
        }

        // Update SEO details
        if (request.SeoDetail != null)
        {
            if (client.ClientSeoDetail != null)
            {
                client.ClientSeoDetail.KeywordRange = request.SeoDetail.KeywordRange;
                client.ClientSeoDetail.Location = request.SeoDetail.Location;
                client.ClientSeoDetail.KeywordsList = request.SeoDetail.KeywordsList;
            }
            else
            {
                var seoDetail = new ClientSeoDetail
                {
                    ClientId = client.Id,
                    KeywordRange = request.SeoDetail.KeywordRange,
                    Location = request.SeoDetail.Location,
                    KeywordsList = request.SeoDetail.KeywordsList
                };
                await _context.ClientSeoDetails.AddAsync(seoDetail);
            }
        }

        // Update AdWords details
        if (request.AdwordsDetail != null)
        {
            if (client.ClientAdwordsDetail != null)
            {
                client.ClientAdwordsDetail.NumberOfKeywords = request.AdwordsDetail.NumberOfKeywords;
                client.ClientAdwordsDetail.Period = request.AdwordsDetail.Period;
                client.ClientAdwordsDetail.Location = request.AdwordsDetail.Location;
                client.ClientAdwordsDetail.KeywordsList = request.AdwordsDetail.KeywordsList;
                client.ClientAdwordsDetail.SpecialGuidelines = request.AdwordsDetail.SpecialGuidelines;
            }
            else
            {
                var adwordsDetail = new ClientAdwordsDetail
                {
                    ClientId = client.Id,
                    NumberOfKeywords = request.AdwordsDetail.NumberOfKeywords,
                    Period = request.AdwordsDetail.Period,
                    Location = request.AdwordsDetail.Location,
                    KeywordsList = request.AdwordsDetail.KeywordsList,
                    SpecialGuidelines = request.AdwordsDetail.SpecialGuidelines
                };
                await _context.ClientAdwordsDetails.AddAsync(adwordsDetail);
            }
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(client.Id) ?? client;
    }

    public async System.Threading.Tasks.Task DeleteAsync(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null || client.IsDeleted)
        {
            throw new KeyNotFoundException("Client not found");
        }

        // Soft delete
        client.IsDeleted = true;
        client.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task<bool> CustomerNoExistsAsync(string customerNo, int? excludeId = null)
    {
        var query = _context.Clients.Where(c => c.CustomerNo == customerNo && !c.IsDeleted);
        
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async System.Threading.Tasks.Task<Client> ApproveClientAsync(int id, int approvedByUserId)
    {
        var client = await GetByIdAsync(id);
        if (client == null)
        {
            throw new KeyNotFoundException("Client not found");
        }

        // Verify approver has permission (Admin, Owner, or Calling Staff)
        var approver = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == approvedByUserId && !u.IsDeleted);

        if (approver?.Role == null)
        {
            throw new UnauthorizedAccessException("Approver not found");
        }

        var roleName = approver.Role.Name;
        var canApprove = roleName == "Admin" || roleName == "Owner" || roleName == "Calling Staff";

        if (!canApprove)
        {
            throw new UnauthorizedAccessException("You do not have permission to approve clients");
        }

        // Update status to Approved
        client.Status = "Approved";
        client.UpdatedAt = DateTime.UtcNow;
        client.UpdatedBy = approver.Name;

        await _context.SaveChangesAsync();

        // Send SignalR notification
        if (_signalRNotificationService != null)
        {
            var approvedClient = await GetByIdAsync(client.Id);
            if (approvedClient != null)
            {
                await _signalRNotificationService.NotifyClientApprovalAsync(approvedClient, approvedByUserId);
            }
        }

        // Trigger workflows
        if (_workflowService != null)
        {
            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    await _workflowService.ExecuteWorkflowsAsync("Client", "StatusChanged", client.Id, client);
                }
                catch (Exception ex)
                {
                    // Log but don't fail approval
                }
            });
        }

        return await GetByIdAsync(client.Id) ?? client;
    }
}

