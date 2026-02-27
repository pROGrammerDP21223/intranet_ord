using backend_net.Data.Context;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public class UserClientService : IUserClientService
{
    private readonly ApplicationDbContext _context;

    public UserClientService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task AttachUserToClientAsync(AttachUserToClientRequest request)
    {
        // Verify user exists
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null || user.IsDeleted)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Verify client exists
        var client = await _context.Clients.FindAsync(request.ClientId);
        if (client == null || client.IsDeleted)
        {
            throw new KeyNotFoundException("Client not found");
        }

        // Check if already attached
        var existing = await _context.UserClients
            .FirstOrDefaultAsync(uc => uc.UserId == request.UserId 
                && uc.ClientId == request.ClientId 
                && !uc.IsDeleted);

        if (existing != null)
        {
            throw new InvalidOperationException("User is already attached to this client");
        }

        var userClient = new UserClient
        {
            UserId = request.UserId,
            ClientId = request.ClientId
        };

        await _context.UserClients.AddAsync(userClient);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DetachUserFromClientAsync(AttachUserToClientRequest request)
    {
        var userClient = await _context.UserClients
            .FirstOrDefaultAsync(uc => uc.UserId == request.UserId 
                && uc.ClientId == request.ClientId 
                && !uc.IsDeleted);

        if (userClient == null)
        {
            throw new KeyNotFoundException("User is not attached to this client");
        }

        // Soft delete
        userClient.IsDeleted = true;
        userClient.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<Client>> GetUserClientsAsync(int userId)
    {
        return await _context.UserClients
            .Include(uc => uc.Client)
            .Where(uc => uc.UserId == userId && !uc.IsDeleted && uc.Client != null && !uc.Client.IsDeleted)
            .Select(uc => uc.Client!)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<User>> GetClientUsersAsync(int clientId)
    {
        return await _context.UserClients
            .Include(uc => uc.User)
            .ThenInclude(u => u!.Role)
            .Where(uc => uc.ClientId == clientId && !uc.IsDeleted && uc.User != null && !uc.User.IsDeleted)
            .Select(uc => uc.User!)
            .ToListAsync();
    }

    public async Task<bool> IsUserAttachedToClientAsync(int userId, int clientId)
    {
        return await _context.UserClients
            .AnyAsync(uc => uc.UserId == userId 
                && uc.ClientId == clientId 
                && !uc.IsDeleted);
    }
}

public class SalesPersonClientService : ISalesPersonClientService
{
    private readonly ApplicationDbContext _context;

    public SalesPersonClientService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task AttachSalesPersonToClientAsync(AttachSalesPersonToClientRequest request)
    {
        // Verify sales person exists and has Sales Person role
        var salesPerson = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.SalesPersonId && !u.IsDeleted);
        
        if (salesPerson == null)
        {
            throw new KeyNotFoundException("Sales person not found");
        }

        if (salesPerson.Role?.Name != "Sales Person")
        {
            throw new InvalidOperationException("User is not a Sales Person");
        }

        // Verify client exists
        var client = await _context.Clients.FindAsync(request.ClientId);
        if (client == null || client.IsDeleted)
        {
            throw new KeyNotFoundException("Client not found");
        }

        // Check if already attached
        var existing = await _context.SalesPersonClients
            .FirstOrDefaultAsync(spc => spc.SalesPersonId == request.SalesPersonId 
                && spc.ClientId == request.ClientId 
                && !spc.IsDeleted);

        if (existing != null)
        {
            throw new InvalidOperationException("Sales person is already attached to this client");
        }

        var salesPersonClient = new SalesPersonClient
        {
            SalesPersonId = request.SalesPersonId,
            ClientId = request.ClientId
        };

        await _context.SalesPersonClients.AddAsync(salesPersonClient);
        
        // Update client status based on assignment logic
        await UpdateClientStatusOnAssignmentAsync(client, salesPerson, request.SalesPersonId);
        
        await _context.SaveChangesAsync();
    }

    private async System.Threading.Tasks.Task UpdateClientStatusOnAssignmentAsync(Client client, User salesPerson, int assignedSalesPersonId)
    {
        if (client == null || salesPerson == null)
            return;

        // If client was created by a Sales Person or Sales Manager
        if (client.CreatedByUserId.HasValue && client.Status == "Pending")
        {
            var creator = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == client.CreatedByUserId.Value && !u.IsDeleted);

            if (creator?.Role != null)
            {
                var creatorRole = creator.Role.Name;

                // If created by Sales Person and assigned to different Sales Person
                if (creatorRole == "Sales Person" && creator.Id != assignedSalesPersonId)
                {
                    client.Status = "AssignedToSomeoneElse";
                    client.AssignedToSalesPersonName = null;
                }
                // If created by Sales Manager
                else if (creatorRole == "Sales Manager")
                {
                    // Check if assigned sales person is under this manager
                    var isUnderManager = await _context.SalesManagerSalesPersons
                        .AnyAsync(smsp => smsp.SalesManagerId == creator.Id 
                                     && smsp.SalesPersonId == assignedSalesPersonId 
                                     && !smsp.IsDeleted);

                    if (isUnderManager)
                    {
                        // Assigned to sales person under this manager - show name
                        client.Status = "Approved";
                        client.AssignedToSalesPersonName = salesPerson.Name;
                    }
                    else
                    {
                        // Assigned to someone else
                        client.Status = "AssignedToSomeoneElse";
                        client.AssignedToSalesPersonName = null;
                    }
                }
            }
        }
        // If client was already approved and being assigned
        else if (client.Status == "Approved" || client.Status == "AssignedToSomeoneElse")
        {
            // Check who is assigning (should be Admin/Owner/Calling Staff)
            // Just update the assigned name
            client.AssignedToSalesPersonName = salesPerson.Name;
            client.Status = "Approved";
        }
    }

    public async System.Threading.Tasks.Task AttachMultipleSalesPersonsToClientAsync(AttachMultipleSalesPersonsToClientRequest request)
    {
        // Verify client exists
        var client = await _context.Clients.FindAsync(request.ClientId);
        if (client == null || client.IsDeleted)
        {
            throw new KeyNotFoundException("Client not found");
        }

        // Verify all sales persons exist and have Sales Person role
        var salesPersonIds = request.SalesPersonIds.Distinct().ToList();
        var salesPersons = await _context.Users
            .Include(u => u.Role)
            .Where(u => salesPersonIds.Contains(u.Id) && !u.IsDeleted)
            .ToListAsync();

        var invalidIds = salesPersonIds.Except(salesPersons.Select(sp => sp.Id)).ToList();
        if (invalidIds.Any())
        {
            throw new KeyNotFoundException($"One or more sales persons not found. Missing IDs: {string.Join(", ", invalidIds)}");
        }

        var nonSalesPersons = salesPersons.Where(sp => sp.Role?.Name != "Sales Person").ToList();
        if (nonSalesPersons.Any())
        {
            throw new InvalidOperationException($"One or more users are not Sales Persons. User IDs: {string.Join(", ", nonSalesPersons.Select(sp => sp.Id))}");
        }

        // Get existing attachments
        var existingAttachments = await _context.SalesPersonClients
            .Where(spc => spc.ClientId == request.ClientId 
                && salesPersonIds.Contains(spc.SalesPersonId) 
                && !spc.IsDeleted)
            .Select(spc => spc.SalesPersonId)
            .ToListAsync();

        // Filter out already attached
        var salesPersonsToAttach = salesPersonIds.Except(existingAttachments).ToList();

        if (salesPersonsToAttach.Count == 0)
        {
            throw new InvalidOperationException("All selected sales persons are already attached to this client");
        }

        // Create new attachments
        var salesPersonClients = salesPersonsToAttach.Select(salesPersonId => new SalesPersonClient
        {
            SalesPersonId = salesPersonId,
            ClientId = request.ClientId
        }).ToList();

        await _context.SalesPersonClients.AddRangeAsync(salesPersonClients);
        
        // Update client status for the first assigned sales person
        if (salesPersonClients.Any() && client != null)
        {
            var firstSalesPerson = salesPersons.FirstOrDefault(sp => sp.Id == salesPersonClients.First().SalesPersonId);
            if (firstSalesPerson != null)
            {
                await UpdateClientStatusOnAssignmentAsync(client, firstSalesPerson, firstSalesPerson.Id);
            }
        }
        
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DetachSalesPersonFromClientAsync(AttachSalesPersonToClientRequest request)
    {
        var salesPersonClient = await _context.SalesPersonClients
            .FirstOrDefaultAsync(spc => spc.SalesPersonId == request.SalesPersonId 
                && spc.ClientId == request.ClientId 
                && !spc.IsDeleted);

        if (salesPersonClient == null)
        {
            throw new KeyNotFoundException("Sales person is not attached to this client");
        }

        // Soft delete
        salesPersonClient.IsDeleted = true;
        salesPersonClient.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<Client>> GetSalesPersonClientsAsync(int salesPersonId)
    {
        return await _context.SalesPersonClients
            .Include(spc => spc.Client)
            .Where(spc => spc.SalesPersonId == salesPersonId && !spc.IsDeleted && spc.Client != null && !spc.Client.IsDeleted)
            .Select(spc => spc.Client!)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<User>> GetClientSalesPersonsAsync(int clientId)
    {
        return await _context.SalesPersonClients
            .Include(spc => spc.SalesPerson)
            .ThenInclude(sp => sp!.Role)
            .Where(spc => spc.ClientId == clientId && !spc.IsDeleted && spc.SalesPerson != null && !spc.SalesPerson.IsDeleted)
            .Select(spc => spc.SalesPerson!)
            .ToListAsync();
    }

    public async Task<bool> IsSalesPersonAttachedToClientAsync(int salesPersonId, int clientId)
    {
        return await _context.SalesPersonClients
            .AnyAsync(spc => spc.SalesPersonId == salesPersonId 
                && spc.ClientId == clientId 
                && !spc.IsDeleted);
    }
}

public class SalesManagerSalesPersonService : ISalesManagerSalesPersonService
{
    private readonly ApplicationDbContext _context;

    public SalesManagerSalesPersonService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task AttachSalesManagerToSalesPersonAsync(AttachSalesManagerToSalesPersonRequest request)
    {
        // Verify sales manager exists and has Sales Manager role
        var salesManager = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.SalesManagerId && !u.IsDeleted);
        
        if (salesManager == null)
        {
            throw new KeyNotFoundException("Sales manager not found");
        }

        if (salesManager.Role?.Name != "Sales Manager")
        {
            throw new InvalidOperationException("User is not a Sales Manager");
        }

        // Verify sales person exists and has Sales Person role
        var salesPerson = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.SalesPersonId && !u.IsDeleted);
        
        if (salesPerson == null)
        {
            throw new KeyNotFoundException("Sales person not found");
        }

        if (salesPerson.Role?.Name != "Sales Person")
        {
            throw new InvalidOperationException("User is not a Sales Person");
        }

        // Check if already attached
        var existing = await _context.SalesManagerSalesPersons
            .FirstOrDefaultAsync(smsp => smsp.SalesManagerId == request.SalesManagerId 
                && smsp.SalesPersonId == request.SalesPersonId 
                && !smsp.IsDeleted);

        if (existing != null)
        {
            throw new InvalidOperationException("Sales manager is already attached to this sales person");
        }

        var salesManagerSalesPerson = new SalesManagerSalesPerson
        {
            SalesManagerId = request.SalesManagerId,
            SalesPersonId = request.SalesPersonId
        };

        await _context.SalesManagerSalesPersons.AddAsync(salesManagerSalesPerson);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DetachSalesManagerFromSalesPersonAsync(AttachSalesManagerToSalesPersonRequest request)
    {
        var salesManagerSalesPerson = await _context.SalesManagerSalesPersons
            .FirstOrDefaultAsync(smsp => smsp.SalesManagerId == request.SalesManagerId 
                && smsp.SalesPersonId == request.SalesPersonId 
                && !smsp.IsDeleted);

        if (salesManagerSalesPerson == null)
        {
            throw new KeyNotFoundException("Sales manager is not attached to this sales person");
        }

        // Soft delete
        salesManagerSalesPerson.IsDeleted = true;
        salesManagerSalesPerson.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<User>> GetSalesManagerSalesPersonsAsync(int salesManagerId)
    {
        return await _context.SalesManagerSalesPersons
            .Include(smsp => smsp.SalesPerson)
            .ThenInclude(sp => sp!.Role)
            .Where(smsp => smsp.SalesManagerId == salesManagerId && !smsp.IsDeleted && smsp.SalesPerson != null && !smsp.SalesPerson.IsDeleted)
            .Select(smsp => smsp.SalesPerson!)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<User>> GetSalesPersonManagersAsync(int salesPersonId)
    {
        return await _context.SalesManagerSalesPersons
            .Include(smsp => smsp.SalesManager)
            .ThenInclude(sm => sm!.Role)
            .Where(smsp => smsp.SalesPersonId == salesPersonId && !smsp.IsDeleted && smsp.SalesManager != null && !smsp.SalesManager.IsDeleted)
            .Select(smsp => smsp.SalesManager!)
            .ToListAsync();
    }

    public async Task<bool> IsSalesManagerAttachedToSalesPersonAsync(int salesManagerId, int salesPersonId)
    {
        return await _context.SalesManagerSalesPersons
            .AnyAsync(smsp => smsp.SalesManagerId == salesManagerId 
                && smsp.SalesPersonId == salesPersonId 
                && !smsp.IsDeleted);
    }
}

public class SalesManagerClientService : ISalesManagerClientService
{
    private readonly ApplicationDbContext _context;

    public SalesManagerClientService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task AttachSalesManagerToClientAsync(AttachSalesManagerToClientRequest request)
    {
        var salesManager = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.SalesManagerId && !u.IsDeleted);

        if (salesManager == null)
            throw new KeyNotFoundException("Sales manager not found");
        if (salesManager.Role?.Name != "Sales Manager")
            throw new InvalidOperationException("User is not a Sales Manager");

        var client = await _context.Clients.FindAsync(request.ClientId);
        if (client == null || client.IsDeleted)
            throw new KeyNotFoundException("Client not found");

        var existing = await _context.SalesManagerClients
            .FirstOrDefaultAsync(smc => smc.SalesManagerId == request.SalesManagerId
                && smc.ClientId == request.ClientId && !smc.IsDeleted);
        if (existing != null)
            throw new InvalidOperationException("Sales manager is already attached to this client");

        await _context.SalesManagerClients.AddAsync(new SalesManagerClient
        {
            SalesManagerId = request.SalesManagerId,
            ClientId = request.ClientId
        });
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DetachSalesManagerFromClientAsync(AttachSalesManagerToClientRequest request)
    {
        var smc = await _context.SalesManagerClients
            .FirstOrDefaultAsync(s => s.SalesManagerId == request.SalesManagerId
                && s.ClientId == request.ClientId && !s.IsDeleted);
        if (smc == null)
            throw new KeyNotFoundException("Sales manager is not attached to this client");
        smc.IsDeleted = true;
        smc.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<Client>> GetSalesManagerClientsAsync(int salesManagerId)
    {
        return await _context.SalesManagerClients
            .Include(smc => smc.Client)
            .Where(smc => smc.SalesManagerId == salesManagerId && !smc.IsDeleted && smc.Client != null && !smc.Client.IsDeleted)
            .Select(smc => smc.Client!)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<User>> GetClientSalesManagersAsync(int clientId)
    {
        return await _context.SalesManagerClients
            .Include(smc => smc.SalesManager)
            .ThenInclude(sm => sm!.Role)
            .Where(smc => smc.ClientId == clientId && !smc.IsDeleted && smc.SalesManager != null && !smc.SalesManager.IsDeleted)
            .Select(smc => smc.SalesManager!)
            .ToListAsync();
    }

    public async Task<bool> IsSalesManagerAttachedToClientAsync(int salesManagerId, int clientId)
    {
        return await _context.SalesManagerClients
            .AnyAsync(smc => smc.SalesManagerId == salesManagerId && smc.ClientId == clientId && !smc.IsDeleted);
    }
}

public class OwnerClientService : IOwnerClientService
{
    private readonly ApplicationDbContext _context;

    public OwnerClientService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task AttachOwnerToClientAsync(AttachOwnerToClientRequest request)
    {
        var owner = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.OwnerId && !u.IsDeleted);

        if (owner == null)
            throw new KeyNotFoundException("Owner not found");
        if (owner.Role?.Name != "Owner")
            throw new InvalidOperationException("User is not an Owner");

        var client = await _context.Clients.FindAsync(request.ClientId);
        if (client == null || client.IsDeleted)
            throw new KeyNotFoundException("Client not found");

        var existing = await _context.OwnerClients
            .FirstOrDefaultAsync(oc => oc.OwnerId == request.OwnerId && oc.ClientId == request.ClientId && !oc.IsDeleted);
        if (existing != null)
            throw new InvalidOperationException("Owner is already attached to this client");

        await _context.OwnerClients.AddAsync(new OwnerClient
        {
            OwnerId = request.OwnerId,
            ClientId = request.ClientId
        });
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DetachOwnerFromClientAsync(AttachOwnerToClientRequest request)
    {
        var oc = await _context.OwnerClients
            .FirstOrDefaultAsync(o => o.OwnerId == request.OwnerId && o.ClientId == request.ClientId && !o.IsDeleted);
        if (oc == null)
            throw new KeyNotFoundException("Owner is not attached to this client");
        oc.IsDeleted = true;
        oc.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<Client>> GetOwnerClientsAsync(int ownerId)
    {
        return await _context.OwnerClients
            .Include(oc => oc.Client)
            .Where(oc => oc.OwnerId == ownerId && !oc.IsDeleted && oc.Client != null && !oc.Client.IsDeleted)
            .Select(oc => oc.Client!)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<User>> GetClientOwnersAsync(int clientId)
    {
        return await _context.OwnerClients
            .Include(oc => oc.Owner)
            .ThenInclude(o => o!.Role)
            .Where(oc => oc.ClientId == clientId && !oc.IsDeleted && oc.Owner != null && !oc.Owner.IsDeleted)
            .Select(oc => oc.Owner!)
            .ToListAsync();
    }

    public async Task<bool> IsOwnerAttachedToClientAsync(int ownerId, int clientId)
    {
        return await _context.OwnerClients
            .AnyAsync(oc => oc.OwnerId == ownerId && oc.ClientId == clientId && !oc.IsDeleted);
    }
}

