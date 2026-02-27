using backend_net.Data.Context;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace backend_net.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmailTemplateService> _logger;

    public EmailTemplateService(ApplicationDbContext context, ILogger<EmailTemplateService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<EmailTemplate>> GetAllAsync()
    {
        return await _context.EmailTemplates
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<EmailTemplate?> GetByIdAsync(int id)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<EmailTemplate?> GetByTypeAsync(string templateType)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.TemplateType == templateType && t.IsActive && !t.IsDeleted);
    }

    public async Task<EmailTemplate> CreateAsync(CreateEmailTemplateRequest request)
    {
        var template = new EmailTemplate
        {
            Name = request.Name,
            Subject = request.Subject,
            Body = request.Body,
            TemplateType = request.TemplateType,
            Description = request.Description,
            IsActive = request.IsActive,
            Variables = request.Variables,
            CreatedAt = DateTime.UtcNow
        };

        await _context.EmailTemplates.AddAsync(template);
        await _context.SaveChangesAsync();

        return template;
    }

    public async Task<EmailTemplate> UpdateAsync(int id, UpdateEmailTemplateRequest request)
    {
        var template = await GetByIdAsync(id);
        if (template == null)
        {
            throw new KeyNotFoundException($"Email template with ID {id} not found");
        }

        if (!string.IsNullOrEmpty(request.Name))
            template.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Subject))
            template.Subject = request.Subject;

        if (!string.IsNullOrEmpty(request.Body))
            template.Body = request.Body;

        if (!string.IsNullOrEmpty(request.TemplateType))
            template.TemplateType = request.TemplateType;

        if (request.Description != null)
            template.Description = request.Description;

        if (request.IsActive.HasValue)
            template.IsActive = request.IsActive.Value;

        if (request.Variables != null)
            template.Variables = request.Variables;

        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return template;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var template = await GetByIdAsync(id);
        if (template == null)
        {
            return false;
        }

        template.IsDeleted = true;
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public Task<string> RenderTemplateAsync(EmailTemplate template, Dictionary<string, string> variables)
    {
        var (_, body) = RenderTemplateCore(template, variables);
        return System.Threading.Tasks.Task.FromResult(body);
    }

    public Task<(string Subject, string Body)> RenderWithSubjectAsync(EmailTemplate template, Dictionary<string, string> variables)
    {
        var result = RenderTemplateCore(template, variables);
        return System.Threading.Tasks.Task.FromResult(result);
    }

    private static (string Subject, string Body) RenderTemplateCore(EmailTemplate template, Dictionary<string, string> variables)
    {
        var subject = template.Subject ?? string.Empty;
        var body = template.Body ?? string.Empty;

        foreach (var variable in variables)
        {
            var placeholder = $"{{{{{variable.Key}}}}}";
            var value = variable.Value ?? string.Empty;
            subject = subject.Replace(placeholder, value);
            body = body.Replace(placeholder, value);
        }

        return (subject, body);
    }
}

