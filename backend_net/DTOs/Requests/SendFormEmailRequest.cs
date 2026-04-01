namespace backend_net.DTOs.Requests;

public class SendFormEmailRequest
{
    public List<string> Emails { get; set; } = new();
}
