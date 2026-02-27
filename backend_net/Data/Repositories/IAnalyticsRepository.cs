using backend_net.Domain.Entities;

namespace backend_net.Data.Repositories;

/// <summary>
/// Repository interface for Analytics data access
/// Follows Repository pattern and Interface Segregation Principle
/// </summary>
public interface IAnalyticsRepository
{
    // Client queries
    Task<int> GetClientCountAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<int> GetNewClientCountAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalPackageValueAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    
    // Transaction queries
    Task<int> GetTransactionCountAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalPaymentsAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRefundsAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetAverageTransactionAmountAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    
    // Revenue by period
    Task<List<(string Period, decimal Revenue, int TransactionCount, int ClientCount)>> GetRevenueByPeriodAsync(
        List<int>? clientIds = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        string period = "monthly",
        CancellationToken cancellationToken = default);
    
    // Top clients
    Task<List<(int ClientId, string ClientName, string CustomerNo, decimal TotalRevenue, decimal PackageValue, int TransactionCount)>> GetTopClientsByRevenueAsync(
        List<int>? clientIds = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int topCount = 10,
        CancellationToken cancellationToken = default);
    
    // Transaction breakdowns
    Task<List<(string Type, int Count, decimal TotalAmount)>> GetTransactionsByTypeAsync(
        List<int>? clientIds = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
    
    Task<List<(string PaymentMethod, int Count, decimal TotalAmount)>> GetTransactionsByPaymentMethodAsync(
        List<int>? clientIds = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
    
    // Growth metrics
    Task<decimal> GetRevenueGrowthAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetClientGrowthRateAsync(List<int>? clientIds = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

