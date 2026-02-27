using backend_net.Constants;
using backend_net.Data.Context;
using backend_net.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public static class EmailTemplateSeederService
{
    public static async System.Threading.Tasks.Task SeedEmailTemplatesAsync(ApplicationDbContext context)
    {
        var existingCount = await context.EmailTemplates.CountAsync(t => !t.IsDeleted);
        if (existingCount > 0)
        {
            Console.WriteLine("[SeedEmailTemplates] Templates already exist. Skipping seeding.");
            return;
        }

        var templates = new List<EmailTemplate>
        {
            CreatePasswordResetTemplate(),
            CreateEnquiryAutoReplyTemplate(),
            CreateEnquiryNotificationToAdminTemplate(),
            CreateEnquiryNotificationToClientTemplate(),
            CreateTicketCreatedTemplate(),
            CreateTicketAssignedTemplate(),
            CreateClientCreatedClientTemplate(),
            CreateClientCreatedCreatorTemplate(),
            CreateClientCreatedOwnerTemplate()
        };

        await context.EmailTemplates.AddRangeAsync(templates);
        await context.SaveChangesAsync();

        Console.WriteLine($"[SeedEmailTemplates] SUCCESS: {templates.Count} email templates seeded.");
    }

    private static EmailTemplate CreatePasswordResetTemplate() => new()
    {
        Name = "Password Reset",
        Subject = "Password Reset Request - One Rank Digital",
        Body = @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #0b51b7 0%, #7081b9 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }
        .button { display: inline-block; padding: 12px 30px; background-color: #0b51b7; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }
        .footer { text-align: center; margin-top: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header""><h2>Password Reset Request</h2></div>
        <div class=""content"">
            <p>Hello {{name}},</p>
            <p>We received a request to reset your password for your One Rank Digital account.</p>
            <p>Click the button below to reset your password:</p>
            <div style=""text-align: center;"">
                <a href=""{{fullResetUrl}}"" class=""button"">Reset Password</a>
            </div>
            <p>Or copy and paste this link into your browser:</p>
            <p style=""word-break: break-all; color: #0b51b7;"">{{fullResetUrl}}</p>
            <p><strong>This link will expire in 1 hour.</strong></p>
            <p>If you didn't request a password reset, please ignore this email.</p>
        </div>
        <div class=""footer""><p>&copy; {{year}} One Rank Digital. All rights reserved.</p></div>
    </div>
</body>
</html>",
        TemplateType = EmailTemplateTypes.PasswordReset,
        Description = "Sent when user requests password reset",
        Variables = "[\"name\", \"fullResetUrl\", \"year\"]",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static EmailTemplate CreateEnquiryAutoReplyTemplate() => new()
    {
        Name = "Enquiry Auto-Reply",
        Subject = "Thank You for Your Enquiry",
        Body = @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #0b51b7 0%, #7081b9 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }
        .footer { text-align: center; margin-top: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header""><h2>Thank You for Your Enquiry</h2></div>
        <div class=""content"">
            <p>Dear {{enquiryFullName}},</p>
            <p>Thank you for contacting us. We have received your enquiry and our team will review it shortly.</p>
            <p>We appreciate your interest and will get back to you as soon as possible.</p>
            <p>If you have any urgent questions, please feel free to contact us directly.</p>
            <p>Best regards,<br/>One Rank Digital Team</p>
        </div>
        <div class=""footer""><p>&copy; {{year}} One Rank Digital. All rights reserved.</p></div>
    </div>
</body>
</html>",
        TemplateType = EmailTemplateTypes.EnquiryAutoReply,
        Description = "Auto-reply to enquirer when enquiry is submitted",
        Variables = "[\"enquiryFullName\", \"year\"]",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static EmailTemplate CreateEnquiryNotificationToAdminTemplate() => new()
    {
        Name = "Enquiry Notification to Admin",
        Subject = "New Enquiry Received",
        Body = @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #0b51b7 0%, #7081b9 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }
        .info-box { background: white; padding: 15px; margin: 10px 0; border-left: 4px solid #0b51b7; }
        .footer { text-align: center; margin-top: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header""><h2>New Enquiry Received</h2></div>
        <div class=""content"">
            <p>Hello Admin,</p>
            <p>A new enquiry has been received from a client.</p>
            <div class=""info-box"">
                <p><strong>Client:</strong> {{clientName}}</p>
                <p><strong>Enquirer Name:</strong> {{enquiryFullName}}</p>
                <p><strong>Email:</strong> {{enquiryEmail}}</p>
                <p><strong>Mobile:</strong> {{enquiryMobile}}</p>
                <p><strong>Message:</strong></p>
                <p>{{enquiryMessage}}</p>
            </div>
            <p>Please review and respond to this enquiry at your earliest convenience.</p>
        </div>
        <div class=""footer""><p>&copy; {{year}} One Rank Digital. All rights reserved.</p></div>
    </div>
</body>
</html>",
        TemplateType = EmailTemplateTypes.EnquiryNotificationToAdmin,
        Description = "Notification to admin when new enquiry is received",
        Variables = "[\"clientName\", \"enquiryFullName\", \"enquiryEmail\", \"enquiryMobile\", \"enquiryMessage\", \"year\"]",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static EmailTemplate CreateEnquiryNotificationToClientTemplate() => new()
    {
        Name = "Enquiry Notification to Client",
        Subject = "New Enquiry Received for Your Business",
        Body = @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #0b51b7 0%, #7081b9 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }
        .info-box { background: white; padding: 15px; margin: 10px 0; border-left: 4px solid #0b51b7; }
        .footer { text-align: center; margin-top: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header""><h2>New Enquiry Received</h2></div>
        <div class=""content"">
            <p>Hello {{clientName}},</p>
            <p>A new enquiry has been received for your business.</p>
            <div class=""info-box"">
                <p><strong>Enquirer Name:</strong> {{enquiryFullName}}</p>
                <p><strong>Email:</strong> {{enquiryEmail}}</p>
                <p><strong>Mobile:</strong> {{enquiryMobile}}</p>
                <p><strong>Message:</strong></p>
                <p>{{enquiryMessage}}</p>
            </div>
            <p>We will follow up with the enquirer and keep you informed.</p>
        </div>
        <div class=""footer""><p>&copy; {{year}} One Rank Digital. All rights reserved.</p></div>
    </div>
</body>
</html>",
        TemplateType = EmailTemplateTypes.EnquiryNotificationToClient,
        Description = "Notification to client when enquiry is received for their business",
        Variables = "[\"clientName\", \"enquiryFullName\", \"enquiryEmail\", \"enquiryMobile\", \"enquiryMessage\", \"year\"]",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static EmailTemplate CreateTicketCreatedTemplate() => new()
    {
        Name = "Ticket Created Notification",
        Subject = "New Ticket Created: {{ticketNumber}}",
        Body = @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #0b51b7 0%, #7081b9 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }
        .info-box { background: white; padding: 15px; margin: 10px 0; border-left: 4px solid #0b51b7; }
        .button { display: inline-block; padding: 12px 30px; background-color: #0b51b7; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }
        .footer { text-align: center; margin-top: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header""><h2>New Ticket Created</h2></div>
        <div class=""content"">
            <p>Hello {{recipientName}},</p>
            <p>A new support ticket has been created that you have access to.</p>
            <div class=""info-box"">
                <p><strong>Ticket Number:</strong> {{ticketNumber}}</p>
                <p><strong>Title:</strong> {{ticketTitle}}</p>
                {{clientInfo}}
                <p><strong>Created By:</strong> {{creatorName}}</p>
                <p><strong>Description:</strong></p>
                <p>{{ticketDescription}}</p>
            </div>
            <p>Please review the ticket and take appropriate action if needed.</p>
            <div style=""text-align: center;""><a href=""#"" class=""button"">View Ticket</a></div>
        </div>
        <div class=""footer""><p>&copy; {{year}} One Rank Digital. All rights reserved.</p></div>
    </div>
</body>
</html>",
        TemplateType = EmailTemplateTypes.TicketCreated,
        Description = "Notification when a new support ticket is created",
        Variables = "[\"recipientName\", \"ticketNumber\", \"ticketTitle\", \"clientInfo\", \"creatorName\", \"ticketDescription\", \"year\"]",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static EmailTemplate CreateTicketAssignedTemplate() => new()
    {
        Name = "Ticket Assigned Notification",
        Subject = "Ticket Assigned to You: {{ticketNumber}}",
        Body = @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }
        .info-box { background: white; padding: 15px; margin: 10px 0; border-left: 4px solid #28a745; }
        .button { display: inline-block; padding: 12px 30px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }
        .footer { text-align: center; margin-top: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header""><h2>Ticket Assigned to You</h2></div>
        <div class=""content"">
            <p>Hello {{recipientName}},</p>
            <p>A support ticket has been assigned to you.</p>
            <div class=""info-box"">
                <p><strong>Ticket Number:</strong> {{ticketNumber}}</p>
                <p><strong>Title:</strong> {{ticketTitle}}</p>
                <p><strong>Assigned By:</strong> {{assignedByName}}</p>
            </div>
            <p>Please review the ticket and take appropriate action.</p>
            <div style=""text-align: center;""><a href=""#"" class=""button"">View Ticket</a></div>
        </div>
        <div class=""footer""><p>&copy; {{year}} One Rank Digital. All rights reserved.</p></div>
    </div>
</body>
</html>",
        TemplateType = EmailTemplateTypes.TicketAssigned,
        Description = "Notification when a ticket is assigned to a user",
        Variables = "[\"recipientName\", \"ticketNumber\", \"ticketTitle\", \"assignedByName\", \"year\"]",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static EmailTemplate CreateClientCreatedClientTemplate() => new()
    {
        Name = "Client Created - To Client Company",
        Subject = "Welcome to One Rank Digital - Your Client Profile Has Been Created",
        Body = @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #0b51b7 0%, #7081b9 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }
        .info-box { background: white; padding: 15px; margin: 10px 0; border-left: 4px solid #0b51b7; }
        .footer { text-align: center; margin-top: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header""><h2>Welcome to One Rank Digital</h2></div>
        <div class=""content"">
            <p>Hello {{recipientName}},</p>
            <p>Your client profile for <strong>{{clientCompanyName}}</strong> has been successfully created in our system.</p>
            <div class=""info-box"">
                <p><strong>Company:</strong> {{clientCompanyName}}</p>
                <p><strong>Created By:</strong> {{createdByName}}</p>
            </div>
            <p>You can now access our services and manage your account through the One Rank Digital portal.</p>
            <p>If you have any questions, please don't hesitate to contact us.</p>
            <p>Best regards,<br/>One Rank Digital Team</p>
        </div>
        <div class=""footer""><p>&copy; {{year}} One Rank Digital. All rights reserved.</p></div>
    </div>
</body>
</html>",
        TemplateType = EmailTemplateTypes.ClientCreatedClient,
        Description = "Welcome email to client when their profile is created",
        Variables = "[\"recipientName\", \"clientCompanyName\", \"createdByName\", \"year\"]",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static EmailTemplate CreateClientCreatedCreatorTemplate() => new()
    {
        Name = "Client Created - To Creator",
        Subject = "Client Created Successfully: {{clientCompanyName}}",
        Body = @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }
        .info-box { background: white; padding: 15px; margin: 10px 0; border-left: 4px solid #28a745; }
        .footer { text-align: center; margin-top: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header""><h2>Client Created Successfully</h2></div>
        <div class=""content"">
            <p>Hello {{recipientName}},</p>
            <p>You have successfully created a new client in the One Rank Digital system.</p>
            <div class=""info-box"">
                <p><strong>Client Company:</strong> {{clientCompanyName}}</p>
            </div>
            <p>The client and owner have been notified. You can view and manage this client from your dashboard.</p>
            <p>Best regards,<br/>One Rank Digital</p>
        </div>
        <div class=""footer""><p>&copy; {{year}} One Rank Digital. All rights reserved.</p></div>
    </div>
</body>
</html>",
        TemplateType = EmailTemplateTypes.ClientCreatedCreator,
        Description = "Confirmation to user who created the client",
        Variables = "[\"recipientName\", \"clientCompanyName\", \"year\"]",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static EmailTemplate CreateClientCreatedOwnerTemplate() => new()
    {
        Name = "Client Created - To Owner",
        Subject = "New Client Created: {{clientCompanyName}}",
        Body = @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #0b51b7 0%, #7081b9 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }
        .info-box { background: white; padding: 15px; margin: 10px 0; border-left: 4px solid #0b51b7; }
        .footer { text-align: center; margin-top: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header""><h2>New Client Created</h2></div>
        <div class=""content"">
            <p>Hello {{recipientName}},</p>
            <p>A new client has been added to the One Rank Digital system.</p>
            <div class=""info-box"">
                <p><strong>Client Company:</strong> {{clientCompanyName}}</p>
                <p><strong>Created By:</strong> {{createdByName}}</p>
            </div>
            <p>Please review the client details in your dashboard when convenient.</p>
            <p>Best regards,<br/>One Rank Digital</p>
        </div>
        <div class=""footer""><p>&copy; {{year}} One Rank Digital. All rights reserved.</p></div>
    </div>
</body>
</html>",
        TemplateType = EmailTemplateTypes.ClientCreatedOwner,
        Description = "Notification to owner when new client is created",
        Variables = "[\"recipientName\", \"clientCompanyName\", \"createdByName\", \"year\"]",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };
}
