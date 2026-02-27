using backend_net.Data.Context;
using backend_net.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Data.Repositories;

/// <summary>
/// Repository implementation for Analytics data access
/// Follows Repository pattern - encapsulates data access logic
/// </summary>
public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly ApplicationDbContext _context;

    public AnalyticsRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<int> GetClientCountAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Clients.Where(c => !c.IsDeleted);

        if (clientIds != null && clientIds.Any())
        {
            query = query.Where(c => clientIds.Contains(c.Id));
        }

        if (startDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= endDate.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> GetNewClientCountAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Clients.Where(c => !c.IsDeleted);

        if (clientIds != null && clientIds.Any())
        {
            query = query.Where(c => clientIds.Contains(c.Id));
        }

        if (startDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= endDate.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalPackageValueAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Clients.Where(c => !c.IsDeleted && c.TotalPackage.HasValue);

        if (clientIds != null && clientIds.Any())
        {
            query = query.Where(c => clientIds.Contains(c.Id));
        }

        if (startDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= endDate.Value);
        }

        return await query.SumAsync(c => c.TotalPackage ?? 0, cancellationToken);
    }

    public async Task<int> GetTransactionCountAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions.Where(t => !t.IsDeleted);

        if (clientIds != null && clientIds.Any())
        {
            query = query.Where(t => clientIds.Contains(t.ClientId));
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalRevenueAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions.Where(t => !t.IsDeleted);

        if (clientIds != null && clientIds.Any())
        {
            query = query.Where(t => clientIds.Contains(t.ClientId));
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        }

        return await query.SumAsync(t => t.Amount, cancellationToken);
    }

    public async Task<decimal> GetTotalPaymentsAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .Where(t => !t.IsDeleted && t.TransactionType == TransactionTypes.Payment && t.Amount > 0);

        if (clientIds != null && clientIds.Any())
        {
            query = query.Where(t => clientIds.Contains(t.ClientId));
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        }

        return await query.SumAsync(t => t.Amount, cancellationToken);
    }

    public async Task<decimal> GetTotalRefundsAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .Where(t => !t.IsDeleted && (t.TransactionType == TransactionTypes.Refund || t.Amount < 0));

        if (clientIds != null && clientIds.Any())
        {
            query = query.Where(t => clientIds.Contains(t.ClientId));
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        }

        return Math.Abs(await query.SumAsync(t => t.Amount, cancellationToken));
    }

    public async Task<decimal> GetAverageTransactionAmountAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions.Where(t => !t.IsDeleted);

        if (clientIds != null && clientIds.Any())
        {
            query = query.Where(t => clientIds.Contains(t.ClientId));
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        }

        var average = await query.AverageAsync(t => (double?)t.Amount, cancellationToken);
        return average.HasValue ? (decimal)average.Value : 0;
    }

    public async Task<List<(string Period, decimal Revenue, int TransactionCount, int ClientCount)>> GetRevenueByPeriodAsync(
        List<int>? clientIds = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string period = "monthly",
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions.Where(t => !t.IsDeleted);

        if (clientIds != null && clientIds.Any())
        {
            query = query.Where(t => clientIds.Contains(t.ClientId));
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        }

        var transactions = await query.ToListAsync(cancellationToken);

        var grouped = period.ToLower() switch
        {
            "yearly" => transactions.GroupBy(t => t.TransactionDate.Year.ToString()),
            "daily" => transactions.GroupBy(t => t.TransactionDate.ToString("yyyy-MM-dd")),
            _ => transactions.GroupBy(t => $"{t.TransactionDate.Year}-{t.TransactionDate.Month:D2}")
        };

        var result = grouped.Select(g => new
        {
            Period = g.Key,
            Revenue = g.Sum(t => t.Amount),
            TransactionCount = g.Count(),
            ClientCount = g.Select(t => t.ClientId).Distinct().Count()
        }).ToList();

        return result.Select(r => (r.Period, r.Revenue, r.TransactionCount, r.ClientCount)).ToList();
    }

    public async Task<List<(int ClientId, string ClientName, string CustomerNo, decimal TotalRevenue, decimal PackageValue, int TransactionCount)>> GetTopClientsByRevenueAsync(
        List<int>? clientIds = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int topCount = 10,
        CancellationToken cancellationToken = default)
    {
        var transactionQuery = _context.Transactions.Where(t => !t.IsDeleted);

        if (clientIds != null && clientIds.Any())
        {
            transactionQuery = transactionQuery.Where(t => clientIds.Contains(t.ClientId));
        }

        if (startDate.HasValue)
        {
            transactionQuery = transactionQuery.Where(t => t.TransactionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            transactionQuery = transactionQuery.Where(t => t.TransactionDate <= endDate.Value);
        }

        var clientQuery = _context.Clients.Where(c => !c.IsDeleted);

        if (clientIds != null && clientIds.Any())
        {
            clientQuery = clientQuery.Where(c => clientIds.Contains(c.Id));
        }

        var transactions = await transactionQuery
            .GroupBy(t => t.ClientId)
            .Select(g => new
            {
                ClientId = g.Key,
                TotalRevenue = g.Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(topCount)
            .ToListAsync(cancellationToken);

        var clientIdsList = transactions.Select(t => t.ClientId).ToList();
        var clients = await clientQuery
            .Where(c => clientIdsList.Contains(c.Id))
            .Select(c => new
            {
                c.Id,
                c.CompanyName,
                c.CustomerNo,
                c.TotalPackage
            })
            .ToListAsync(cancellationToken);

        return transactions.Select(t =>
        {
            var client = clients.FirstOrDefault(c => c.Id == t.ClientId);
            return (
                t.ClientId,
                client?.CompanyName ?? "Unknown",
                client?.CustomerNo ?? "",
                t.TotalRevenue,
                client?.TotalPackage ?? 0,
                t.TransactionCount
            );
        }).ToList();
    }

    public async Task<List<(string Type, int Count, decimal TotalAmount)>> GetTransactionsByTypeAsync(
        List<int>? clientIds = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions.Where(t => !t.IsDeleted);

        if (clientIds != null && clientIds.Any())
        {
            query = query.Where(t => clientIds.Contains(t.ClientId));
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        }

        var result = await query
            .GroupBy(t => t.TransactionType)
            .Select(g => new
            {
                Type = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(t => t.Amount)
            })
            .ToListAsync(cancellationToken);

        return result.Select(r => (r.Type, r.Count, r.TotalAmount)).ToList();
    }

    public async Task<List<(string PaymentMethod, int Count, decimal TotalAmount)>> GetTransactionsByPaymentMethodAsync(
        List<int>? clientIds = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .Where(t => !t.IsDeleted && !string.IsNullOrEmpty(t.PaymentMethod));

        if (clientIds != null && clientIds.Any())
        {
            query = query.Where(t => clientIds.Contains(t.ClientId));
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= endDate.Value);
        }

        var result = await query
            .GroupBy(t => t.PaymentMethod!)
            .Select(g => new
            {
                PaymentMethod = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(t => t.Amount)
            })
            .ToListAsync(cancellationToken);

        return result.Select(r => (r.PaymentMethod, r.Count, r.TotalAmount)).ToList();
    }

    public async Task<decimal> GetRevenueGrowthAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        if (!startDate.HasValue || !endDate.HasValue)
        {
            return 0;
        }

        var periodDays = (endDate.Value - startDate.Value).Days;
        if (periodDays <= 0)
        {
            return 0;
        }

        var previousPeriodStart = startDate.Value.AddDays(-periodDays);
        var previousPeriodEnd = startDate.Value;

        var currentRevenue = await GetTotalRevenueAsync(clientIds, startDate, endDate, cancellationToken);
        var previousRevenue = await GetTotalRevenueAsync(clientIds, previousPeriodStart, previousPeriodEnd, cancellationToken);

        if (previousRevenue == 0)
        {
            return currentRevenue > 0 ? 100 : 0;
        }

        return ((currentRevenue - previousRevenue) / previousRevenue) * 100;
    }

    public async Task<decimal> GetClientGrowthRateAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        if (!startDate.HasValue || !endDate.HasValue)
        {
            return 0;
        }

        var periodDays = (endDate.Value - startDate.Value).Days;
        if (periodDays <= 0)
        {
            return 0;
        }

        var previousPeriodStart = startDate.Value.AddDays(-periodDays);
        var previousPeriodEnd = startDate.Value;

        var currentClients = await GetClientCountAsync(clientIds, startDate, endDate, cancellationToken);
        var previousClients = await GetClientCountAsync(clientIds, previousPeriodStart, previousPeriodEnd, cancellationToken);

        if (previousClients == 0)
        {
            return currentClients > 0 ? 100 : 0;
        }

        return ((currentClients - previousClients) / (decimal)previousClients) * 100;
    }
}

