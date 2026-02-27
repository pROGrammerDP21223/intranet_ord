namespace backend_net.DTOs.Responses;

public class AnalyticsResponse
{
    public OverviewMetrics Overview { get; set; } = new();
    public RevenueMetrics Revenue { get; set; } = new();
    public ClientMetrics Clients { get; set; } = new();
    public TransactionMetrics Transactions { get; set; } = new();
    public List<RevenueByPeriod> RevenueByPeriod { get; set; } = new();
    public List<TopClientByRevenue> TopClients { get; set; } = new();
    public List<TransactionByType> TransactionsByType { get; set; } = new();
    public List<TransactionByPaymentMethod> TransactionsByPaymentMethod { get; set; } = new();
}

public class OverviewMetrics
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalPackageValue { get; set; }
    public int TotalClients { get; set; }
    public int TotalTransactions { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public decimal AverageClientValue { get; set; }
}

public class RevenueMetrics
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal TotalRefunds { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal RevenueGrowth { get; set; } // Percentage
}

public class ClientMetrics
{
    public int TotalClients { get; set; }
    public int NewClientsThisMonth { get; set; }
    public int NewClientsThisYear { get; set; }
    public decimal AverageClientValue { get; set; }
    public decimal ClientGrowthRate { get; set; } // Percentage
}

public class TransactionMetrics
{
    public int TotalTransactions { get; set; }
    public int PaymentCount { get; set; }
    public int RefundCount { get; set; }
    public decimal TotalPaymentAmount { get; set; }
    public decimal TotalRefundAmount { get; set; }
    public decimal AverageTransactionAmount { get; set; }
}

public class RevenueByPeriod
{
    public string Period { get; set; } = string.Empty; // "2024-01", "2024", etc.
    public decimal Revenue { get; set; }
    public int TransactionCount { get; set; }
    public int ClientCount { get; set; }
}

public class TopClientByRevenue
{
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string CustomerNo { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal PackageValue { get; set; }
    public int TransactionCount { get; set; }
}

public class TransactionByType
{
    public string TransactionType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
}

public class TransactionByPaymentMethod
{
    public string PaymentMethod { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
}

