namespace backend_net.DTOs.Requests;

public class GetAnalyticsRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Period { get; set; } // "monthly", "yearly", "daily"
}

