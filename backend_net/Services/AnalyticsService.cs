using backend_net.Data.Repositories;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.DTOs.Responses;
using backend_net.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend_net.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _repository;
    private readonly IAccessControlService _accessControlService;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IAnalyticsRepository repository,
        IAccessControlService accessControlService,
        ILogger<AnalyticsService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _accessControlService = accessControlService ?? throw new ArgumentNullException(nameof(accessControlService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AnalyticsResponse> GetAnalyticsAsync(int userId, GetAnalyticsRequest? request = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get accessible client IDs based on user role
            var accessibleClientIds = await _accessControlService.GetAccessibleClientIdsAsync(userId);
            List<int>? clientIds = accessibleClientIds.Any() ? accessibleClientIds : null;

            // Set default date range if not provided (last 12 months)
            var endDate = request?.EndDate ?? DateTime.UtcNow;
            var startDate = request?.StartDate ?? endDate.AddMonths(-12);
            var period = request?.Period ?? "monthly";

            // Calculate date ranges for growth metrics
            var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var currentYearStart = new DateTime(DateTime.UtcNow.Year, 1, 1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var previousYearStart = currentYearStart.AddYears(-1);

            // Overview metrics
            var totalClients = await _repository.GetClientCountAsync(clientIds, startDate, endDate, cancellationToken);
            var totalTransactions = await _repository.GetTransactionCountAsync(clientIds, startDate, endDate, cancellationToken);
            var totalRevenue = await _repository.GetTotalRevenueAsync(clientIds, startDate, endDate, cancellationToken);
            var totalPackageValue = await _repository.GetTotalPackageValueAsync(clientIds, startDate, endDate, cancellationToken);
            var averageTransactionAmount = await _repository.GetAverageTransactionAmountAsync(clientIds, startDate, endDate, cancellationToken);
            var averageClientValue = totalClients > 0 ? totalPackageValue / totalClients : 0;

            // Revenue metrics
            var totalPayments = await _repository.GetTotalPaymentsAsync(clientIds, startDate, endDate, cancellationToken);
            var totalRefunds = await _repository.GetTotalRefundsAsync(clientIds, startDate, endDate, cancellationToken);
            var outstandingAmount = totalPackageValue - totalPayments + totalRefunds;
            var revenueGrowth = await _repository.GetRevenueGrowthAsync(clientIds, startDate, endDate, cancellationToken);

            // Client metrics
            var newClientsThisMonth = await _repository.GetNewClientCountAsync(clientIds, currentMonthStart, DateTime.UtcNow, cancellationToken);
            var newClientsThisYear = await _repository.GetNewClientCountAsync(clientIds, currentYearStart, DateTime.UtcNow, cancellationToken);
            var clientGrowthRate = await _repository.GetClientGrowthRateAsync(clientIds, startDate, endDate, cancellationToken);

            // Transaction metrics
            var paymentCount = await _repository.GetTransactionCountAsync(
                clientIds,
                startDate,
                endDate,
                cancellationToken);

            // Get transaction breakdowns
            var transactionsByType = await _repository.GetTransactionsByTypeAsync(clientIds, startDate, endDate, cancellationToken);
            var transactionsByPaymentMethod = await _repository.GetTransactionsByPaymentMethodAsync(clientIds, startDate, endDate, cancellationToken);

            // Calculate percentages for transaction breakdowns
            var totalTransactionAmount = transactionsByType.Sum(t => t.TotalAmount);
            var transactionsByTypeWithPercentage = transactionsByType.Select(t => new TransactionByType
            {
                TransactionType = t.Type,
                Count = t.Count,
                TotalAmount = t.TotalAmount,
                Percentage = totalTransactionAmount != 0 ? (t.TotalAmount / totalTransactionAmount) * 100 : 0
            }).ToList();

            var transactionsByPaymentMethodWithPercentage = transactionsByPaymentMethod.Select(t => new TransactionByPaymentMethod
            {
                PaymentMethod = t.PaymentMethod ?? "Unknown",
                Count = t.Count,
                TotalAmount = t.TotalAmount,
                Percentage = totalPayments != 0 ? (t.TotalAmount / totalPayments) * 100 : 0
            }).ToList();

            // Revenue by period
            var revenueByPeriodData = await _repository.GetRevenueByPeriodAsync(clientIds, startDate, endDate, period, cancellationToken);
            var revenueByPeriod = revenueByPeriodData.Select(r => new RevenueByPeriod
            {
                Period = r.Period,
                Revenue = r.Revenue,
                TransactionCount = r.TransactionCount,
                ClientCount = r.ClientCount
            }).ToList();

            // Top clients
            var topClientsData = await _repository.GetTopClientsByRevenueAsync(clientIds, startDate, endDate, 10, cancellationToken);
            var topClients = topClientsData.Select(c => new TopClientByRevenue
            {
                ClientId = c.ClientId,
                ClientName = c.ClientName,
                CustomerNo = c.CustomerNo,
                TotalRevenue = c.TotalRevenue,
                PackageValue = c.PackageValue,
                TransactionCount = c.TransactionCount
            }).ToList();

            // Count payment and refund transactions
            var paymentTransactions = transactionsByType.FirstOrDefault(t => t.Type == TransactionTypes.Payment);
            var refundTransactions = transactionsByType.FirstOrDefault(t => t.Type == TransactionTypes.Refund);
            
            var paymentTransactionCount = paymentTransactions.Type == TransactionTypes.Payment ? paymentTransactions.Count : 0;
            var refundTransactionCount = refundTransactions.Type == TransactionTypes.Refund ? refundTransactions.Count : 0;

            return new AnalyticsResponse
            {
                Overview = new OverviewMetrics
                {
                    TotalRevenue = totalRevenue,
                    TotalPackageValue = totalPackageValue,
                    TotalClients = totalClients,
                    TotalTransactions = totalTransactions,
                    AverageTransactionAmount = averageTransactionAmount,
                    AverageClientValue = averageClientValue
                },
                Revenue = new RevenueMetrics
                {
                    TotalRevenue = totalRevenue,
                    TotalPayments = totalPayments,
                    TotalRefunds = totalRefunds,
                    OutstandingAmount = outstandingAmount,
                    RevenueGrowth = revenueGrowth
                },
                Clients = new ClientMetrics
                {
                    TotalClients = totalClients,
                    NewClientsThisMonth = newClientsThisMonth,
                    NewClientsThisYear = newClientsThisYear,
                    AverageClientValue = averageClientValue,
                    ClientGrowthRate = clientGrowthRate
                },
                Transactions = new TransactionMetrics
                {
                    TotalTransactions = totalTransactions,
                    PaymentCount = paymentTransactionCount,
                    RefundCount = refundTransactionCount,
                    TotalPaymentAmount = totalPayments,
                    TotalRefundAmount = totalRefunds,
                    AverageTransactionAmount = averageTransactionAmount
                },
                RevenueByPeriod = revenueByPeriod,
                TopClients = topClients,
                TransactionsByType = transactionsByTypeWithPercentage,
                TransactionsByPaymentMethod = transactionsByPaymentMethodWithPercentage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics for user {UserId}", userId);
            throw;
        }
    }
}

