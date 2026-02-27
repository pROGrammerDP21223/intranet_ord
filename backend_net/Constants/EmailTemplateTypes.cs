namespace backend_net.Constants;

/// <summary>
/// Template type constants for EmailTemplate table. Used with GetByTypeAsync.
/// </summary>
public static class EmailTemplateTypes
{
    public const string PasswordReset = "password_reset";
    public const string EnquiryAutoReply = "enquiry_auto_reply";
    public const string EnquiryNotificationToAdmin = "enquiry_notification_to_admin";
    public const string EnquiryNotificationToClient = "enquiry_notification_to_client";
    public const string TicketCreated = "ticket_created";
    public const string TicketAssigned = "ticket_assigned";
    public const string ClientCreatedClient = "client_created_client";
    public const string ClientCreatedCreator = "client_created_creator";
    public const string ClientCreatedOwner = "client_created_owner";
}
