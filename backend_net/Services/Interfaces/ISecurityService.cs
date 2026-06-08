namespace backend_net.Services.Interfaces;

public interface ISecurityService
{
    Task<bool> ValidateCaptchaAsync(string captchaToken, string? remoteIp = null);
    (string CaptchaId, string Question) CreateCustomCaptchaChallenge();
    bool ValidateCustomCaptchaAnswer(string captchaId, string captchaAnswer);
    bool ValidateCsrfToken(string csrfToken, string? sessionToken = null);
    bool ValidateHoneypot(string? honeypotValue);
}

