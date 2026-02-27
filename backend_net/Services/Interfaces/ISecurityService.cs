namespace backend_net.Services.Interfaces;

public interface ISecurityService
{
    Task<bool> ValidateCaptchaAsync(string captchaToken, string? remoteIp = null);
    bool ValidateCsrfToken(string csrfToken, string? sessionToken = null);
    bool ValidateHoneypot(string? honeypotValue);
}

