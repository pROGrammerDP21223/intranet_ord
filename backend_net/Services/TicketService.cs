using backend_net.Data.Context;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace backend_net.Services;

public class TicketService : ITicketService
{
    private readonly ApplicationDbContext _context;
    private readonly IAccessControlService _accessControlService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ISignalRNotificationService? _signalRNotificationService;
    private readonly IWorkflowService? _workflowService;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        ApplicationDbContext context, 
        IAccessControlService accessControlService,
        IEmailService emailService,
        IConfiguration configuration,
        ISignalRNotificationService? signalRNotificationService,
        IWorkflowService? workflowService,
        ILogger<TicketService> logger)
    {
        _context = context;
        _accessControlService = accessControlService;
        _emailService = emailService;
        _configuration = configuration;
        _signalRNotificationService = signalRNotificationService;
        _workflowService = workflowService;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<Ticket?> GetByIdAsync(int id, int? userId = null)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Creator)
                .ThenInclude(u => u!.Role)
            .Include(t => t.Assignee)
                .ThenInclude(u => u!.Role)
            .Include(t => t.Client)
            .Include(t => t.Comments!)
                .ThenInclude(c => c.User)
                    .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        if (ticket == null)
            return null;

        // Check access if userId is provided
        if (userId.HasValue && !await CanUserAccessTicketAsync(id, userId.Value))
            return null;

        // Filter comments based on user role
        if (userId.HasValue)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted);

            if (user?.Role != null)
            {
                var roleName = user.Role.Name;
                // Only staff/admin/owner can see internal comments
                var canSeeInternal = roleName == "Admin" || roleName == "Owner" || 
                                    roleName == "HOD" || roleName == "Calling Staff" || 
                                    roleName == "Employee";

                if (!canSeeInternal && ticket.Comments != null)
                {
                    ticket.Comments = ticket.Comments.Where(c => !c.IsInternal).ToList();
                }
            }
        }

        return ticket;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Ticket>> GetAllAsync(int? userId = null)
    {
        if (!userId.HasValue)
        {
            // If no userId, return all tickets (for admin/owner)
            return await _context.Tickets
                .Include(t => t.Creator)
                    .ThenInclude(u => u!.Role)
                .Include(t => t.Assignee)
                    .ThenInclude(u => u!.Role)
                .Include(t => t.Client)
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted);

        if (user?.Role == null)
            return new List<Ticket>();

        var roleName = user.Role.Name;
        var canSeeInternal = roleName == "Admin" || roleName == "Owner" || 
                            roleName == "HOD" || roleName == "Calling Staff" || 
                            roleName == "Employee";

        IQueryable<Ticket> query = _context.Tickets
            .Include(t => t.Creator)
                .ThenInclude(u => u!.Role)
            .Include(t => t.Assignee)
                .ThenInclude(u => u!.Role)
            .Include(t => t.Client)
            .Where(t => !t.IsDeleted);

        // Apply role-based filtering
        if (roleName == "Admin" || roleName == "Owner" || roleName == "HOD" || 
            roleName == "Calling Staff" || roleName == "Employee")
        {
            // Staff/Admin/Owner can see all tickets
            query = query.OrderByDescending(t => t.CreatedAt);
        }
        else if (roleName == "Client")
        {
            // Clients can only see their own tickets
            var clientIds = await _context.UserClients
                .Where(uc => uc.UserId == userId.Value && !uc.IsDeleted)
                .Select(uc => uc.ClientId)
                .ToListAsync();

            query = query.Where(t => t.CreatedBy == userId.Value || 
                                     (t.ClientId.HasValue && clientIds.Contains(t.ClientId.Value)))
                         .OrderByDescending(t => t.CreatedAt);
        }
        else if (roleName == "Sales Person")
        {
            // Sales Person can see their own tickets and their clients' tickets
            var clientIds = await _context.SalesPersonClients
                .Where(spc => spc.SalesPersonId == userId.Value && !spc.IsDeleted)
                .Select(spc => spc.ClientId)
                .ToListAsync();

            query = query.Where(t => t.CreatedBy == userId.Value || 
                                     (t.ClientId.HasValue && clientIds.Contains(t.ClientId.Value)))
                         .OrderByDescending(t => t.CreatedAt);
        }
        else if (roleName == "Sales Manager")
        {
            // Sales Manager can see their own tickets, their clients' tickets, 
            // and tickets from their sales persons and their sales persons' clients
            var managerClientIds = await _context.SalesPersonClients
                .Where(spc => spc.SalesPersonId == userId.Value && !spc.IsDeleted)
                .Select(spc => spc.ClientId)
                .ToListAsync();

            var salesPersonIds = await _context.SalesManagerSalesPersons
                .Where(smsp => smsp.SalesManagerId == userId.Value && !smsp.IsDeleted)
                .Select(smsp => smsp.SalesPersonId)
                .ToListAsync();

            var salesPersonClientIds = new List<int>();
            if (salesPersonIds.Any())
            {
                salesPersonClientIds = await _context.SalesPersonClients
                    .Where(spc => salesPersonIds.Contains(spc.SalesPersonId) && !spc.IsDeleted)
                    .Select(spc => spc.ClientId)
                    .Distinct()
                    .ToListAsync();
            }

            var allClientIds = managerClientIds.Union(salesPersonClientIds).Distinct().ToList();

            query = query.Where(t => t.CreatedBy == userId.Value || 
                                     salesPersonIds.Contains(t.CreatedBy) ||
                                     (t.ClientId.HasValue && allClientIds.Contains(t.ClientId.Value)))
                         .OrderByDescending(t => t.CreatedAt);
        }
        else
        {
            // Unknown role - return empty
            return new List<Ticket>();
        }

        var tickets = await query.ToListAsync();

        // Filter internal comments
        if (!canSeeInternal)
        {
            foreach (var ticket in tickets)
            {
                if (ticket.Comments != null)
                {
                    ticket.Comments = ticket.Comments.Where(c => !c.IsInternal).ToList();
                }
            }
        }

        return tickets;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Ticket>> GetTicketsByClientAsync(int clientId, int? userId = null)
    {
        // Check if user can access this client
        if (userId.HasValue && !await _accessControlService.CanUserAccessClientAsync(userId.Value, clientId))
        {
            return new List<Ticket>();
        }

        var tickets = await _context.Tickets
            .Include(t => t.Creator)
                .ThenInclude(u => u!.Role)
            .Include(t => t.Assignee)
                .ThenInclude(u => u!.Role)
            .Include(t => t.Client)
            .Where(t => t.ClientId == clientId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        // Filter internal comments if needed
        if (userId.HasValue)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted);

            if (user?.Role != null)
            {
                var roleName = user.Role.Name;
                var canSeeInternal = roleName == "Admin" || roleName == "Owner" || 
                                    roleName == "HOD" || roleName == "Calling Staff" || 
                                    roleName == "Employee";

                if (!canSeeInternal)
                {
                    foreach (var ticket in tickets)
                    {
                        if (ticket.Comments != null)
                        {
                            ticket.Comments = ticket.Comments.Where(c => !c.IsInternal).ToList();
                        }
                    }
                }
            }
        }

        return tickets;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(string status, int? userId = null)
    {
        var allTickets = await GetAllAsync(userId);
        return allTickets.Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
    }

    public async System.Threading.Tasks.Task<Ticket> CreateAsync(CreateTicketRequest request, int createdByUserId)
    {
        // Generate unique ticket number
        var ticketNumber = await GenerateTicketNumberAsync();

        var ticket = new Ticket
        {
            TicketNumber = ticketNumber,
            Title = request.Title,
            Description = request.Description,
            Status = "Open",
            Priority = request.Priority ?? "Medium",
            CreatedBy = createdByUserId,
            ClientId = request.ClientId
        };

        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();

        // Load ticket with related data for email notifications
        var ticketWithDetails = await _context.Tickets
            .Include(t => t.Creator)
            .Include(t => t.Client)
            .FirstOrDefaultAsync(t => t.Id == ticket.Id);

        // Send email notifications asynchronously (fire and forget)
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                await SendTicketCreatedEmailsAsync(ticketWithDetails!);
                
                        // Send SignalR notification
                        if (_signalRNotificationService != null && ticketWithDetails != null)
                        {
                            await _signalRNotificationService.NotifyTicketUpdateAsync(ticketWithDetails, "created");
                        }

                        // Trigger workflows
                        if (_workflowService != null && ticketWithDetails != null)
                        {
                            await _workflowService.ExecuteWorkflowsAsync("Ticket", "Created", ticketWithDetails.Id, ticketWithDetails);
                        }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ticket created email notifications for ticket {TicketId}", ticket.Id);
            }
        });

        return await GetByIdAsync(ticket.Id, createdByUserId) ?? ticket;
    }

    public async System.Threading.Tasks.Task<Ticket> UpdateAsync(int id, UpdateTicketRequest request, int? userId = null)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        if (ticket == null)
            throw new KeyNotFoundException("Ticket not found");

        // Check access
        if (userId.HasValue && !await CanUserAccessTicketAsync(id, userId.Value))
        {
            throw new UnauthorizedAccessException("You do not have permission to update this ticket");
        }

        if (!string.IsNullOrEmpty(request.Title))
            ticket.Title = request.Title;

        if (!string.IsNullOrEmpty(request.Description))
            ticket.Description = request.Description;

        if (!string.IsNullOrEmpty(request.Status))
            ticket.Status = request.Status;

        if (!string.IsNullOrEmpty(request.Priority))
            ticket.Priority = request.Priority;

        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Send SignalR notification
        if (_signalRNotificationService != null)
        {
            var updatedTicket = await GetByIdAsync(ticket.Id, userId);
            if (updatedTicket != null)
            {
                await _signalRNotificationService.NotifyTicketUpdateAsync(updatedTicket, "updated");
            }
        }

        // Trigger workflows
        if (_workflowService != null)
        {
            var updatedTicket = await GetByIdAsync(ticket.Id, userId);
            if (updatedTicket != null)
            {
                _ = System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        await _workflowService.ExecuteWorkflowsAsync("Ticket", "Updated", updatedTicket.Id, updatedTicket);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing workflows for ticket {TicketId}", updatedTicket.Id);
                    }
                });
            }
        }

        return await GetByIdAsync(ticket.Id, userId) ?? ticket;
    }

    public async System.Threading.Tasks.Task<Ticket> AssignTicketAsync(int ticketId, AssignTicketRequest request, int assignedByUserId)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId && !t.IsDeleted);

        if (ticket == null)
            throw new KeyNotFoundException("Ticket not found");

        // Check if user can assign tickets (Staff/Admin/Owner)
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == assignedByUserId && !u.IsDeleted);

        if (user?.Role == null)
            throw new UnauthorizedAccessException("User not found");

        var roleName = user.Role.Name;
        var canAssign = roleName == "Admin" || roleName == "Owner" || 
                       roleName == "HOD" || roleName == "Calling Staff";

        if (!canAssign)
            throw new UnauthorizedAccessException("You do not have permission to assign tickets");

        // Verify assigned user exists
        var assignedUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.AssignedToUserId && !u.IsDeleted);

        if (assignedUser == null)
            throw new KeyNotFoundException("Assigned user not found");

        ticket.AssignedTo = request.AssignedToUserId;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Load ticket with related data for email notification
        var ticketWithDetails = await _context.Tickets
            .Include(t => t.Assignee)
            .Include(t => t.Creator)
            .FirstOrDefaultAsync(t => t.Id == ticket.Id);

        var assignedByUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == assignedByUserId && !u.IsDeleted);

        // Send SignalR notification
        if (_signalRNotificationService != null && ticketWithDetails != null)
        {
            await _signalRNotificationService.NotifyTicketUpdateAsync(ticketWithDetails, "assigned");
        }

        // Send email notification to assigned user asynchronously (fire and forget)
        if (ticketWithDetails?.Assignee != null && assignedUser != null)
        {
            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendTicketAssignedNotificationAsync(
                        assignedUser.Email,
                        assignedUser.Name,
                        ticketWithDetails.TicketNumber,
                        ticketWithDetails.Title,
                        assignedByUser?.Name ?? "System"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending ticket assigned email notification for ticket {TicketId}", ticket.Id);
                }
            });
        }

        return await GetByIdAsync(ticket.Id, assignedByUserId) ?? ticket;
    }

    public async System.Threading.Tasks.Task<TicketComment> AddCommentAsync(int ticketId, AddTicketCommentRequest request, int userId)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId && !t.IsDeleted);

        if (ticket == null)
            throw new KeyNotFoundException("Ticket not found");

        // Check access
        if (!await CanUserAccessTicketAsync(ticketId, userId))
        {
            throw new UnauthorizedAccessException("You do not have permission to comment on this ticket");
        }

        var comment = new TicketComment
        {
            TicketId = ticketId,
            UserId = userId,
            Comment = request.Comment,
            IsInternal = request.IsInternal
        };

        await _context.TicketComments.AddAsync(comment);
        await _context.SaveChangesAsync();

        return await _context.TicketComments
            .Include(c => c.User)
                .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(c => c.Id == comment.Id) ?? comment;
    }

    public async System.Threading.Tasks.Task<IEnumerable<TicketComment>> GetCommentsAsync(int ticketId, int? userId = null)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId && !t.IsDeleted);

        if (ticket == null)
            throw new KeyNotFoundException("Ticket not found");

        // Check access
        if (userId.HasValue && !await CanUserAccessTicketAsync(ticketId, userId.Value))
        {
            throw new UnauthorizedAccessException("You do not have permission to view comments for this ticket");
        }

        var comments = await _context.TicketComments
            .Include(c => c.User)
                .ThenInclude(u => u!.Role)
            .Where(c => c.TicketId == ticketId && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        // Filter internal comments if needed
        if (userId.HasValue)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted);

            if (user?.Role != null)
            {
                var roleName = user.Role.Name;
                var canSeeInternal = roleName == "Admin" || roleName == "Owner" || 
                                    roleName == "HOD" || roleName == "Calling Staff" || 
                                    roleName == "Employee";

                if (!canSeeInternal)
                {
                    comments = comments.Where(c => !c.IsInternal).ToList();
                }
            }
        }

        return comments;
    }

    public async System.Threading.Tasks.Task<bool> CanUserAccessTicketAsync(int ticketId, int userId)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Creator)
                .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(t => t.Id == ticketId && !t.IsDeleted);

        if (ticket == null)
            return false;

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user?.Role == null)
            return false;

        var roleName = user.Role.Name;

        // Staff/Admin/Owner can access all tickets
        if (roleName == "Admin" || roleName == "Owner" || roleName == "HOD" || 
            roleName == "Calling Staff" || roleName == "Employee")
            return true;

        // Creator can always access their ticket
        if (ticket.CreatedBy == userId)
            return true;

        // Assigned user can access the ticket
        if (ticket.AssignedTo == userId)
            return true;

        // Client users can access tickets for their client
        if (roleName == "Client" && ticket.ClientId.HasValue)
        {
            var userClient = await _context.UserClients
                .FirstOrDefaultAsync(uc => uc.UserId == userId && 
                                          uc.ClientId == ticket.ClientId.Value && 
                                          !uc.IsDeleted);
            return userClient != null;
        }

        // Sales Person can access tickets for their clients
        if (roleName == "Sales Person" && ticket.ClientId.HasValue)
        {
            var salesPersonClient = await _context.SalesPersonClients
                .FirstOrDefaultAsync(spc => spc.SalesPersonId == userId && 
                                           spc.ClientId == ticket.ClientId.Value && 
                                           !spc.IsDeleted);
            return salesPersonClient != null;
        }

        // Sales Manager can access tickets for their clients and their sales persons' clients
        if (roleName == "Sales Manager")
        {
            if (ticket.ClientId.HasValue)
            {
                // Check if client is directly attached to manager (SalesManagerClients)
                var managerClient = await _context.SalesManagerClients
                    .FirstOrDefaultAsync(smc => smc.SalesManagerId == userId && 
                                              smc.ClientId == ticket.ClientId.Value && 
                                              !smc.IsDeleted);
                if (managerClient != null)
                    return true;

                // Check if client is attached to any of manager's sales persons
                var salesPersonIds = await _context.SalesManagerSalesPersons
                    .Where(smsp => smsp.SalesManagerId == userId && !smsp.IsDeleted)
                    .Select(smsp => smsp.SalesPersonId)
                    .ToListAsync();

                if (salesPersonIds.Any())
                {
                    var hasAccess = await _context.SalesPersonClients
                        .AnyAsync(spc => salesPersonIds.Contains(spc.SalesPersonId) && 
                                        spc.ClientId == ticket.ClientId.Value && 
                                        !spc.IsDeleted);
                    if (hasAccess)
                        return true;
                }
            }

            // Check if ticket was created by one of manager's sales persons
            var managerSalesPersonIds = await _context.SalesManagerSalesPersons
                .Where(smsp => smsp.SalesManagerId == userId && !smsp.IsDeleted)
                .Select(smsp => smsp.SalesPersonId)
                .ToListAsync();

            if (managerSalesPersonIds.Contains(ticket.CreatedBy))
                return true;
        }

        return false;
    }

    public async System.Threading.Tasks.Task DeleteAsync(int id, int? userId = null)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        if (ticket == null)
            throw new KeyNotFoundException("Ticket not found");

        // Check access
        if (userId.HasValue && !await CanUserAccessTicketAsync(id, userId.Value))
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this ticket");
        }

        // Soft delete
        ticket.IsDeleted = true;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private async System.Threading.Tasks.Task<string> GenerateTicketNumberAsync()
    {
        var prefix = "TKT";
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month.ToString("D2");

        // Get the last ticket number for this month
        var lastTicket = await _context.Tickets
            .Where(t => t.TicketNumber.StartsWith($"{prefix}-{year}-{month}"))
            .OrderByDescending(t => t.TicketNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastTicket != null)
        {
            var parts = lastTicket.TicketNumber.Split('-');
            if (parts.Length == 4 && int.TryParse(parts[3], out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
        }

        return $"{prefix}-{year}-{month}-{sequence:D4}";
    }

    private async System.Threading.Tasks.Task SendTicketCreatedEmailsAsync(Ticket ticket)
    {
        if (ticket == null || ticket.Creator == null)
            return;

        var emailTasks = new List<Task<bool>>();
        var creatorName = ticket.Creator.Name;
        var clientName = ticket.Client?.CompanyName;

        // 1. Send email to ticket creator
        if (!string.IsNullOrEmpty(ticket.Creator.Email))
        {
            emailTasks.Add(_emailService.SendTicketCreatedNotificationAsync(
                ticket.Creator.Email,
                ticket.Creator.Name,
                ticket.TicketNumber,
                ticket.Title,
                ticket.Description,
                creatorName,
                clientName
            ));
        }

        // 2. Send email to admin/staff (Admin, Owner, HOD, Calling Staff, Employee)
        var adminStaffUsers = await _context.Users
            .Include(u => u.Role)
            .Where(u => !u.IsDeleted && u.IsActive && u.Role != null &&
                (u.Role.Name == "Admin" || u.Role.Name == "Owner" || 
                 u.Role.Name == "HOD" || u.Role.Name == "Calling Staff" || 
                 u.Role.Name == "Employee"))
            .ToListAsync();

        foreach (var user in adminStaffUsers)
        {
            if (!string.IsNullOrEmpty(user.Email) && user.Id != ticket.CreatedBy)
            {
                emailTasks.Add(_emailService.SendTicketCreatedNotificationAsync(
                    user.Email,
                    user.Name,
                    ticket.TicketNumber,
                    ticket.Title,
                    ticket.Description,
                    creatorName,
                    clientName
                ));
            }
        }

        // 3. If ticket is related to a client, send to client users
        if (ticket.ClientId.HasValue)
        {
            var clientUsers = await _context.UserClients
                .Include(uc => uc.User)
                .Where(uc => uc.ClientId == ticket.ClientId.Value && !uc.IsDeleted && uc.User != null && !uc.User.IsDeleted && uc.User.IsActive)
                .Select(uc => uc.User!)
                .ToListAsync();

            foreach (var user in clientUsers)
            {
                if (!string.IsNullOrEmpty(user.Email) && user.Id != ticket.CreatedBy)
                {
                    emailTasks.Add(_emailService.SendTicketCreatedNotificationAsync(
                        user.Email,
                        user.Name,
                        ticket.TicketNumber,
                        ticket.Title,
                        ticket.Description,
                        creatorName,
                        clientName
                    ));
                }
            }

            // 4. Send to sales persons who have access to this client
            var salesPersonUsers = await _context.SalesPersonClients
                .Include(spc => spc.SalesPerson)
                    .ThenInclude(u => u!.Role)
                .Where(spc => spc.ClientId == ticket.ClientId.Value && !spc.IsDeleted && 
                              spc.SalesPerson != null && !spc.SalesPerson.IsDeleted && 
                              spc.SalesPerson.IsActive && spc.SalesPerson.Role != null &&
                              spc.SalesPerson.Role.Name == "Sales Person")
                .Select(spc => spc.SalesPerson!)
                .ToListAsync();

            foreach (var user in salesPersonUsers)
            {
                if (!string.IsNullOrEmpty(user.Email) && user.Id != ticket.CreatedBy)
                {
                    emailTasks.Add(_emailService.SendTicketCreatedNotificationAsync(
                        user.Email,
                        user.Name,
                        ticket.TicketNumber,
                        ticket.Title,
                        ticket.Description,
                        creatorName,
                        clientName
                    ));
                }
            }

            // 5. Send to sales managers who have access through their sales persons
            var salesManagerIds = await _context.SalesManagerSalesPersons
                .Where(smsp => !smsp.IsDeleted)
                .Select(smsp => smsp.SalesManagerId)
                .Distinct()
                .ToListAsync();

            var salesPersonIdsForClient = await _context.SalesPersonClients
                .Where(spc => spc.ClientId == ticket.ClientId.Value && !spc.IsDeleted)
                .Select(spc => spc.SalesPersonId)
                .ToListAsync();

            var relevantManagerIds = await _context.SalesManagerSalesPersons
                .Where(smsp => salesPersonIdsForClient.Contains(smsp.SalesPersonId) && 
                              !smsp.IsDeleted)
                .Select(smsp => smsp.SalesManagerId)
                .Distinct()
                .ToListAsync();

            var salesManagerUsers = await _context.Users
                .Include(u => u.Role)
                .Where(u => relevantManagerIds.Contains(u.Id) && !u.IsDeleted && 
                           u.IsActive && u.Role != null && u.Role.Name == "Sales Manager")
                .ToListAsync();

            foreach (var user in salesManagerUsers)
            {
                if (!string.IsNullOrEmpty(user.Email) && user.Id != ticket.CreatedBy)
                {
                    emailTasks.Add(_emailService.SendTicketCreatedNotificationAsync(
                        user.Email,
                        user.Name,
                        ticket.TicketNumber,
                        ticket.Title,
                        ticket.Description,
                        creatorName,
                        clientName
                    ));
                }
            }
        }

        // Execute all email tasks
        await System.Threading.Tasks.Task.WhenAll(emailTasks);
        _logger.LogInformation("Sent ticket created email notifications for ticket {TicketNumber}", ticket.TicketNumber);
    }
}

