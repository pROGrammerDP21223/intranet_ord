using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MessagesController : BaseController
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
    }

    /// <summary>
    /// Get inbox messages for current user
    /// </summary>
    [HttpGet("inbox")]
    public async Task<IActionResult> GetInbox()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var messages = await _messageService.GetInboxAsync(userId.Value);
            return HandleSuccess("Messages retrieved successfully", messages);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get sent messages for current user
    /// </summary>
    [HttpGet("sent")]
    public async Task<IActionResult> GetSentMessages()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var messages = await _messageService.GetSentMessagesAsync(userId.Value);
            return HandleSuccess("Messages retrieved successfully", messages);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get message by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMessageById(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var message = await _messageService.GetMessageByIdAsync(id, userId.Value);
            if (message == null)
            {
                return HandleError("Message not found", 404);
            }

            return HandleSuccess("Message retrieved successfully", message);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Send a new message
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var message = await _messageService.SendMessageAsync(request, userId.Value);
            return StatusCode(201, new { message = "Message sent successfully", data = message, success = true });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Mark message as read
    /// </summary>
    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var result = await _messageService.MarkAsReadAsync(id, userId.Value);
            if (!result)
            {
                return HandleError("Message not found", 404);
            }

            return HandleSuccess("Message marked as read", null);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Delete a message
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id, [FromQuery] bool isSender = false)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var result = await _messageService.DeleteMessageAsync(id, userId.Value, isSender);
            if (!result)
            {
                return HandleError("Message not found or unauthorized", 404);
            }

            return HandleSuccess("Message deleted successfully", null);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get unread message count
    /// </summary>
    [HttpGet("unread/count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var count = await _messageService.GetUnreadCountAsync(userId.Value);
            return HandleSuccess("Unread count retrieved successfully", new { count });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }
}

