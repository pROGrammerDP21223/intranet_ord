using backend_net.Controllers;
using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend_net.Data.Context;
using System.Net;
using System.Text;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ClientsController : BaseController
{
    private readonly IClientService _clientService;
    private readonly IAccessControlService _accessControlService;
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public ClientsController(IClientService clientService, IAccessControlService accessControlService, ApplicationDbContext context, IEmailService emailService)
    {
        _clientService = clientService;
        _accessControlService = accessControlService;
        _context = context;
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
    {
        try
        {
            // Allow Admin, Owner, Sales Person, and Sales Manager to create clients
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var userRole = GetCurrentUserRole();
            var canCreate = IsAdmin() || IsSalesPerson() || IsSalesManager();
            
            if (!canCreate)
            {
                return HandleError("Unauthorized: Only Admin, Owner, Sales Person, and Sales Manager can create clients", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdByRole = GetCurrentUserRole();
            var client = await _clientService.CreateAsync(request, userId.Value, createdByRole);
            
            // Project to DTO to avoid circular references
            var clientDto = MapToDto(client);
            
            return HandleSuccess("Client created successfully", clientDto);
        }
        catch (InvalidOperationException ex)
        {
            return HandleError(ex.Message, 400);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetClients()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            List<Client> clients;

            // Admin/Owner/HOD/Calling Staff/Employee can see all clients
            if (IsAdmin() || IsHOD() || IsCallingStaff() || IsEmployee())
            {
                clients = (await _clientService.GetAllAsync()).ToList();
            }
            else
            {
                // Get accessible client IDs based on role and relationships
                var accessibleClientIds = await _accessControlService.GetAccessibleClientIdsAsync(userId.Value);
                clients = (await _clientService.GetAllAsync())
                    .Where(c => accessibleClientIds.Contains(c.Id))
                    .ToList();
            }

            var clientsDto = clients.Select(MapToDto).ToList();
            return HandleSuccess("Clients retrieved successfully", clientsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClientById(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Check if user can access this client
            if (!IsAdmin() && !IsHOD() && !IsCallingStaff() && !IsEmployee() && !await _accessControlService.CanUserAccessClientAsync(userId.Value, id))
            {
                return HandleError("Unauthorized: You don't have access to this client", 403);
            }

            var client = await _clientService.GetByIdAsync(id);
            
            if (client == null)
            {
                return HandleError("Client not found", 404);
            }

            var clientDto = MapToDto(client);
            return HandleSuccess("Client retrieved successfully", clientDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClient(int id, [FromBody] CreateClientRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Only Admin/Owner/Sales Person/Sales Manager can edit clients (Clients can only view)
            // But only Admin/Owner/Sales Person/Sales Manager can manage WhatsApp/Email settings
            if (!IsAdmin() && !IsSalesPerson() && !IsSalesManager())
            {
                return HandleError("Unauthorized: Only Admin, Owner, Sales Person, and Sales Manager can edit clients", 403);
            }

            // Check if user can access this client (for Sales Person/Sales Manager)
            if (!IsAdmin() && !await _accessControlService.CanUserAccessClientAsync(userId.Value, id))
            {
                return HandleError("Unauthorized: You don't have access to this client", 403);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = await _clientService.UpdateAsync(id, request);
            var clientDto = MapToDto(client);
            
            return HandleSuccess("Client updated successfully", clientDto);
        }
        catch (KeyNotFoundException ex)
        {
            return HandleError(ex.Message, 404);
        }
        catch (InvalidOperationException ex)
        {
            return HandleError(ex.Message, 400);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        try
        {
            // Only Admin/Owner can delete clients
            if (!IsAdmin())
            {
                return HandleError("Unauthorized: Only Admin and Owner can delete clients", 403);
            }

            await _clientService.DeleteAsync(id);
            return HandleSuccess("Client deleted successfully");
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

    [HttpPatch("{id}/toggle-premium")]
    public async Task<IActionResult> TogglePremium(int id)
    {
        try
        {
            if (!IsAdmin())
                return HandleError("Unauthorized: Only Admin and Owner can change premium status", 403);

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
                return HandleError("Client not found", 404);

            client.IsPremium = !client.IsPremium;
            client.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return HandleSuccess(
                client.IsPremium ? "Client marked as premium" : "Client removed from premium",
                new { client.Id, client.IsPremium }
            );
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    // Helper method to map Entity to DTO (avoids circular references)
    private object MapToDto(Client client)
    {
        var clientServices = client.ClientServices != null && client.ClientServices.Any()
            ? client.ClientServices.Select(cs => new
            {
                cs.Id,
                cs.ServiceId,
                Service = cs.Service != null ? new
                {
                    cs.Service.Id,
                    cs.Service.ServiceType,
                    cs.Service.ServiceName,
                    cs.Service.Category
                } : (object?)null
            }).Cast<object>().ToList()
            : new List<object>();

        var clientEmailServices = client.ClientEmailServices != null && client.ClientEmailServices.Any()
            ? client.ClientEmailServices.Select(es => new
            {
                es.Id,
                es.EmailServiceType,
                es.Quantity
            }).Cast<object>().ToList()
            : new List<object>();

        object? clientSeoDetail = client.ClientSeoDetail != null ? new
        {
            client.ClientSeoDetail.Id,
            client.ClientSeoDetail.KeywordRange,
            client.ClientSeoDetail.Location,
            client.ClientSeoDetail.KeywordsList
        } : null;

        object? clientAdwordsDetail = client.ClientAdwordsDetail != null ? new
        {
            client.ClientAdwordsDetail.Id,
            client.ClientAdwordsDetail.NumberOfKeywords,
            client.ClientAdwordsDetail.Period,
            client.ClientAdwordsDetail.Location,
            client.ClientAdwordsDetail.KeywordsList,
            client.ClientAdwordsDetail.SpecialGuidelines
        } : null;

        return new
        {
            client.Id,
            client.CustomerNo,
            client.FormDate,
            client.AmountWithoutGst,
            client.GstPercentage,
            client.GstAmount,
            client.TotalPackage,
            client.CompanyName,
            client.ContactPerson,
            client.Designation,
            client.Address,
            client.Phone,
            client.Email,
            client.WhatsAppNumber,
            client.EnquiryEmail,
            client.UseWhatsAppService,
            client.WhatsAppSameAsMobile,
            client.UseSameEmailForEnquiries,
            client.DomainName,
            client.CompanyLogo,
            client.GstNo,
            client.SpecificGuidelines,
            client.CreatedAt,
            client.UpdatedAt,
            client.CreatedBy,
            client.CreatedByUserId,
            client.Status,
            client.IsPremium,
            client.AssignedToSalesPersonName,
            ClientServices = clientServices,
            ClientEmailServices = clientEmailServices,
            ClientSeoDetail = clientSeoDetail,
            ClientAdwordsDetail = clientAdwordsDetail
        };
    }

    /// <summary>
    /// Get email recipients info (client email + executive email) for send-form dialog
    /// </summary>
    [HttpGet("{id}/email-info")]
    public async Task<IActionResult> GetEmailInfo(int id)
    {
        try
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null)
                return HandleError("Client not found", 404);

            string? executiveEmail = null;
            string? executiveName = null;
            if (client.CreatedByUserId.HasValue)
            {
                var user = await _context.Users.FindAsync(client.CreatedByUserId.Value);
                executiveEmail = user?.Email;
                executiveName = user?.Name;
            }

            return HandleSuccess("Email info retrieved", new
            {
                clientEmail = client.Email,
                executiveEmail,
                executiveName,
                companyName = client.CompanyName
            });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Send client order form via email to the provided list of addresses
    /// </summary>
    [HttpPost("{id}/send-form-email")]
    public async Task<IActionResult> SendFormEmail(int id, [FromBody] SendFormEmailRequest request)
    {
        try
        {
            if (request.Emails == null || !request.Emails.Any())
                return HandleError("No email addresses provided", 400);

            var client = await _clientService.GetByIdAsync(id);
            if (client == null)
                return HandleError("Client not found", 404);

            var html = GenerateClientFormEmailHtml(client);
            var subject = $"Client Order Form – {client.CompanyName} ({client.CustomerNo})";

            var sent = 0;
            foreach (var email in request.Emails.Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                await _emailService.SendEmailAsync(email.Trim(), subject, html);
                sent++;
            }

            return HandleSuccess($"Email sent to {sent} recipient(s)");
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// Generates an email-client-safe HTML body for the client order form
    private static string GenerateClientFormEmailHtml(Client client)
    {
        var sb = new StringBuilder();

        // ── helpers ──────────────────────────────────────────────────────────
        string Row(string label, string? value) =>
            $"<tr><td style='padding:6px 12px;font-weight:600;color:#555;width:200px;border-bottom:1px solid #f0f0f0;'>{label}</td>" +
            $"<td style='padding:6px 12px;color:#222;border-bottom:1px solid #f0f0f0;'>{System.Net.WebUtility.HtmlEncode(value ?? "—")}</td></tr>";

        string SectionHeader(string title) =>
            $"<tr><td colspan='2' style='padding:10px 12px 4px;font-weight:700;font-size:15px;color:#1a237e;" +
            $"background:#f0f4ff;border-left:4px solid #1a237e;letter-spacing:.5px;'>{title}</td></tr>";

        // ── services ──────────────────────────────────────────────────────────
        var serviceNames = client.ClientServices != null
            ? string.Join(", ", client.ClientServices
                .Where(s => s.Service != null)
                .Select(s => s.Service!.ServiceName))
            : "—";

        var emailServices = client.ClientEmailServices != null && client.ClientEmailServices.Any()
            ? string.Join(", ", client.ClientEmailServices
                .Select(e => $"{e.EmailServiceType} × {e.Quantity}"))
            : "—";

        // ── SEO ───────────────────────────────────────────────────────────────
        var seo = client.ClientSeoDetail;
        var adwords = client.ClientAdwordsDetail;

        // ── date ──────────────────────────────────────────────────────────────
        var formDate = client.FormDate != default
            ? client.FormDate.ToString("dd/MM/yyyy")
            : client.CreatedAt.ToString("dd/MM/yyyy");

        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='UTF-8'></head><body style='margin:0;padding:0;background:#eef0f4;font-family:Arial,sans-serif;'>");
        sb.AppendLine("<table width='100%' cellpadding='0' cellspacing='0' style='background:#eef0f4;'><tr><td align='center' style='padding:30px 10px;'>");
        sb.AppendLine("<table width='700' cellpadding='0' cellspacing='0' style='background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,.1);'>");

        // header
        sb.AppendLine("<tr><td style='background:linear-gradient(135deg,#1a237e,#0d1757);padding:28px 32px;'>");
        sb.AppendLine("<table width='100%' cellpadding='0' cellspacing='0'><tr>");
        sb.AppendLine("<td><span style='font-size:22px;font-weight:800;color:#fff;letter-spacing:1px;'>ONE RANK DIGITAL</span><br>");
        sb.AppendLine("<span style='font-size:12px;color:#ffc107;letter-spacing:2px;'>DIGITAL MARKETING EXPERTS</span></td>");
        sb.AppendLine($"<td align='right' style='color:#fff;font-size:13px;'>Customer No<br><strong style='font-size:16px;color:#ffc107;'>{System.Net.WebUtility.HtmlEncode(client.CustomerNo ?? "—")}</strong><br>");
        sb.AppendLine($"<span style='font-size:12px;color:#ccc;'>Form Date: {formDate}</span></td>");
        sb.AppendLine("</tr></table></td></tr>");

        // body
        sb.AppendLine("<tr><td style='padding:0 24px 24px;'>");
        sb.AppendLine("<table width='100%' cellpadding='0' cellspacing='0' style='border-collapse:collapse;margin-top:20px;font-size:14px;'>");

        sb.AppendLine(SectionHeader("Client Information"));
        sb.AppendLine(Row("Company Name", client.CompanyName));
        sb.AppendLine(Row("Contact Person", client.ContactPerson));
        sb.AppendLine(Row("Designation", client.Designation));
        sb.AppendLine(Row("Address", client.Address));
        sb.AppendLine(Row("Phone", client.Phone));
        sb.AppendLine(Row("Email", client.Email));
        sb.AppendLine(Row("WhatsApp", client.WhatsAppNumber));
        if (!string.IsNullOrWhiteSpace(client.DomainName))
            sb.AppendLine(Row("Domain Name", client.DomainName));
        if (!string.IsNullOrWhiteSpace(client.GstNo))
            sb.AppendLine(Row("GST No", client.GstNo));

        sb.AppendLine(SectionHeader("Package & Billing"));
        sb.AppendLine(Row("Amount (excl. GST)", client.AmountWithoutGst?.ToString("C2", new System.Globalization.CultureInfo("en-IN"))));
        sb.AppendLine(Row("GST", $"{client.GstPercentage}% = {client.GstAmount?.ToString("C2", new System.Globalization.CultureInfo("en-IN"))}"));
        sb.AppendLine(Row("Total Package", client.TotalPackage?.ToString("C2", new System.Globalization.CultureInfo("en-IN"))));

        sb.AppendLine(SectionHeader("Services"));
        sb.AppendLine(Row("Selected Services", serviceNames));
        sb.AppendLine(Row("Email Services", emailServices));

        if (seo != null)
        {
            sb.AppendLine(SectionHeader("SEO Details"));
            sb.AppendLine(Row("Keyword Range", seo.KeywordRange));
            sb.AppendLine(Row("Location", seo.Location));
            if (!string.IsNullOrWhiteSpace(seo.KeywordsList))
                sb.AppendLine(Row("Keywords", seo.KeywordsList));
        }

        if (adwords != null)
        {
            sb.AppendLine(SectionHeader("AdWords Details"));
            sb.AppendLine(Row("No. of Keywords", adwords.NumberOfKeywords?.ToString()));
            sb.AppendLine(Row("Period", adwords.Period));
            sb.AppendLine(Row("Location", adwords.Location));
            if (!string.IsNullOrWhiteSpace(adwords.KeywordsList))
                sb.AppendLine(Row("Keywords", adwords.KeywordsList));
        }

        if (!string.IsNullOrWhiteSpace(client.SpecificGuidelines))
        {
            sb.AppendLine(SectionHeader("Specific Guidelines"));
            sb.AppendLine($"<tr><td colspan='2' style='padding:8px 12px;color:#333;white-space:pre-wrap;'>{System.Net.WebUtility.HtmlEncode(client.SpecificGuidelines)}</td></tr>");
        }

        sb.AppendLine("</table>");

        // footer
        sb.AppendLine("<div style='margin-top:24px;padding:16px;background:#f8f9ff;border-radius:6px;text-align:center;font-size:12px;color:#888;'>");
        sb.AppendLine("This is an automated email from <strong>One Rank Digital</strong>. Please do not reply to this email.");
        sb.AppendLine("</div>");
        sb.AppendLine("</td></tr></table>");
        sb.AppendLine("</td></tr></table></body></html>");

        return sb.ToString();
    }

    /// <summary>
    /// Approve a pending client - Only Admin/Owner/Calling Staff can approve
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveClient(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            // Only Admin/Owner/Calling Staff can approve clients
            if (!IsAdmin() && !IsCallingStaff())
            {
                return HandleError("Unauthorized: Only Admin, Owner, and Calling Staff can approve clients", 403);
            }

            var client = await _clientService.ApproveClientAsync(id, userId.Value);
            var clientDto = MapToDto(client);

            return HandleSuccess("Client approved successfully", clientDto);
        }
        catch (KeyNotFoundException ex)
        {
            return HandleError(ex.Message, 404);
        }
        catch (UnauthorizedAccessException ex)
        {
            return HandleError(ex.Message, 403);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

