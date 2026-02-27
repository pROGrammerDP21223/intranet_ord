using backend_net.Data.Context;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public class AccessControlService : IAccessControlService
{
    private readonly ApplicationDbContext _context;

    public AccessControlService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanUserAccessClientAsync(int userId, int clientId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null || user.Role == null)
            return false;

        var roleName = user.Role.Name;

        // Admin/Owner/HOD/Calling Staff/Employee can access all clients
        if (roleName == "Admin" || roleName == "Owner" || roleName == "HOD" || roleName == "Calling Staff" || roleName == "Employee")
            return true;

        // Client users can only access their own client
        if (roleName == "Client")
        {
            var userClient = await _context.UserClients
                .FirstOrDefaultAsync(uc => uc.UserId == userId 
                    && uc.ClientId == clientId 
                    && !uc.IsDeleted);
            return userClient != null;
        }

        // Sales Person can access their attached clients
        if (roleName == "Sales Person")
        {
            var salesPersonClient = await _context.SalesPersonClients
                .FirstOrDefaultAsync(spc => spc.SalesPersonId == userId 
                    && spc.ClientId == clientId 
                    && !spc.IsDeleted);
            return salesPersonClient != null;
        }

        // Sales Manager can access their own clients + clients of their sales persons
        if (roleName == "Sales Manager")
        {
            // Check if client is directly attached to manager (SalesManagerClients)
            var managerClient = await _context.SalesManagerClients
                .FirstOrDefaultAsync(smc => smc.SalesManagerId == userId 
                    && smc.ClientId == clientId 
                    && !smc.IsDeleted);
            
            if (managerClient != null)
                return true;

            // Check if client is attached to any of manager's sales persons
            var managerSalesPersons = await _context.SalesManagerSalesPersons
                .Where(smsp => smsp.SalesManagerId == userId && !smsp.IsDeleted)
                .Select(smsp => smsp.SalesPersonId)
                .ToListAsync();

            if (managerSalesPersons.Any())
            {
                var hasAccess = await _context.SalesPersonClients
                    .AnyAsync(spc => managerSalesPersons.Contains(spc.SalesPersonId) 
                        && spc.ClientId == clientId 
                        && !spc.IsDeleted);
                return hasAccess;
            }
        }

        return false;
    }

    public async Task<List<int>> GetAccessibleClientIdsAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null || user.Role == null)
            return new List<int>();

        var roleName = user.Role.Name;

        // Admin/Owner/HOD/Calling Staff/Employee can access all clients
        if (roleName == "Admin" || roleName == "Owner" || roleName == "HOD" || roleName == "Calling Staff" || roleName == "Employee")
        {
            return await _context.Clients
                .Where(c => !c.IsDeleted)
                .Select(c => c.Id)
                .ToListAsync();
        }

        // Client users can only access their own client
        if (roleName == "Client")
        {
            return await _context.UserClients
                .Where(uc => uc.UserId == userId && !uc.IsDeleted)
                .Select(uc => uc.ClientId)
                .ToListAsync();
        }

        // Sales Person can access their attached clients
        if (roleName == "Sales Person")
        {
            return await _context.SalesPersonClients
                .Where(spc => spc.SalesPersonId == userId && !spc.IsDeleted)
                .Select(spc => spc.ClientId)
                .ToListAsync();
        }

        // Sales Manager can access their own clients + clients of their sales persons
        if (roleName == "Sales Manager")
        {
            // Get manager's direct clients (from SalesManagerClients)
            var managerClientIds = await _context.SalesManagerClients
                .Where(smc => smc.SalesManagerId == userId && !smc.IsDeleted)
                .Select(smc => smc.ClientId)
                .ToListAsync();

            // Get sales persons under this manager
            var salesPersonIds = await _context.SalesManagerSalesPersons
                .Where(smsp => smsp.SalesManagerId == userId && !smsp.IsDeleted)
                .Select(smsp => smsp.SalesPersonId)
                .ToListAsync();

            // Get clients of those sales persons
            var salesPersonClientIds = new List<int>();
            if (salesPersonIds.Any())
            {
                salesPersonClientIds = await _context.SalesPersonClients
                    .Where(spc => salesPersonIds.Contains(spc.SalesPersonId) && !spc.IsDeleted)
                    .Select(spc => spc.ClientId)
                    .Distinct()
                    .ToListAsync();
            }

            // Combine and return unique client IDs
            return managerClientIds.Union(salesPersonClientIds).Distinct().ToList();
        }

        return new List<int>();
    }

    public async Task<bool> CanUserAttachProductToClientAsync(int userId, int clientId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null || user.Role == null)
            return false;

        var roleName = user.Role.Name;

        // Admin/Owner can attach products to any client
        if (roleName == "Admin" || roleName == "Owner")
            return true;

        // Sales Person can attach products to their attached clients
        if (roleName == "Sales Person")
        {
            var salesPersonClient = await _context.SalesPersonClients
                .FirstOrDefaultAsync(spc => spc.SalesPersonId == userId 
                    && spc.ClientId == clientId 
                    && !spc.IsDeleted);
            return salesPersonClient != null;
        }

        // Sales Manager can attach products to their clients or clients of their sales persons
        if (roleName == "Sales Manager")
        {
            // Check if client is directly attached to manager (SalesManagerClients)
            var managerClient = await _context.SalesManagerClients
                .FirstOrDefaultAsync(smc => smc.SalesManagerId == userId 
                    && smc.ClientId == clientId 
                    && !smc.IsDeleted);
            
            if (managerClient != null)
                return true;

            // Check if client is attached to any of manager's sales persons
            var managerSalesPersons = await _context.SalesManagerSalesPersons
                .Where(smsp => smsp.SalesManagerId == userId && !smsp.IsDeleted)
                .Select(smsp => smsp.SalesPersonId)
                .ToListAsync();

            if (managerSalesPersons.Any())
            {
                var hasAccess = await _context.SalesPersonClients
                    .AnyAsync(spc => managerSalesPersons.Contains(spc.SalesPersonId) 
                        && spc.ClientId == clientId 
                        && !spc.IsDeleted);
                return hasAccess;
            }
        }

        return false;
    }
}

