using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;

namespace backend_net.Services.Interfaces;

public interface IEmailTemplateService
{
    Task<IEnumerable<EmailTemplate>> GetAllAsync();
    Task<EmailTemplate?> GetByIdAsync(int id);
    Task<EmailTemplate?> GetByTypeAsync(string templateType);
    Task<EmailTemplate> CreateAsync(CreateEmailTemplateRequest request);
    Task<EmailTemplate> UpdateAsync(int id, UpdateEmailTemplateRequest request);
    Task<bool> DeleteAsync(int id);
    Task<string> RenderTemplateAsync(EmailTemplate template, Dictionary<string, string> variables);
    
    /// <summary>
    /// Renders both subject and body with the given variables. Placeholders use {{VariableName}} format.
    /// </summary>
    Task<(string Subject, string Body)> RenderWithSubjectAsync(EmailTemplate template, Dictionary<string, string> variables);
}

