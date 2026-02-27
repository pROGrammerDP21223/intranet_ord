using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EmailTemplatesController : BaseController
{
    private readonly IEmailTemplateService _emailTemplateService;

    public EmailTemplatesController(IEmailTemplateService emailTemplateService)
    {
        _emailTemplateService = emailTemplateService ?? throw new ArgumentNullException(nameof(emailTemplateService));
    }

    /// <summary>
    /// Get all email templates
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllTemplates()
    {
        try
        {
            var templates = await _emailTemplateService.GetAllAsync();
            return HandleSuccess("Templates retrieved successfully", templates);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get email template by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTemplateById(int id)
    {
        try
        {
            var template = await _emailTemplateService.GetByIdAsync(id);
            if (template == null)
            {
                return HandleError("Template not found", 404);
            }
            return HandleSuccess("Template retrieved successfully", template);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get email template by type
    /// </summary>
    [HttpGet("type/{templateType}")]
    public async Task<IActionResult> GetTemplateByType(string templateType)
    {
        try
        {
            var template = await _emailTemplateService.GetByTypeAsync(templateType);
            if (template == null)
            {
                return HandleError("Template not found", 404);
            }
            return HandleSuccess("Template retrieved successfully", template);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Create new email template
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateEmailTemplateRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Only Admin/Owner can create templates
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can create email templates", 403);
            }

            var template = await _emailTemplateService.CreateAsync(request);
            return StatusCode(201, new { message = "Template created successfully", data = template, success = true });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Update email template
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTemplate(int id, [FromBody] UpdateEmailTemplateRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Only Admin/Owner can update templates
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can update email templates", 403);
            }

            var template = await _emailTemplateService.UpdateAsync(id, request);
            return HandleSuccess("Template updated successfully", template);
        }
        catch (KeyNotFoundException ex)
        {
            return HandleError(ex.Message, 404);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Delete email template
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTemplate(int id)
    {
        try
        {
            // Only Admin/Owner can delete templates
            if (!IsAdmin() && !IsOwner())
            {
                return HandleError("Unauthorized: Only Admin and Owner can delete email templates", 403);
            }

            var result = await _emailTemplateService.DeleteAsync(id);
            if (!result)
            {
                return HandleError("Template not found", 404);
            }
            return HandleSuccess("Template deleted successfully", null);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

