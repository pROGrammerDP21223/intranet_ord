using backend_net.Controllers;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_net.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TicketsController : BaseController
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>
    /// Create a new ticket - Anyone can create tickets
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
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

            var ticket = await _ticketService.CreateAsync(request, userId.Value);
            var ticketDto = MapToDto(ticket);

            return StatusCode(201, new { message = "Ticket created successfully", data = ticketDto, success = true });
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get all tickets - Role-based filtering applied
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllTickets()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var tickets = await _ticketService.GetAllAsync(userId.Value);
            var ticketsDto = tickets.Select(MapToDto).ToList();

            return HandleSuccess("Tickets retrieved successfully", ticketsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get ticket by ID - Role-based access control applied
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicketById(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var ticket = await _ticketService.GetByIdAsync(id, userId.Value);
            if (ticket == null)
            {
                return HandleError("Ticket not found or you do not have permission to view it", 404);
            }

            var ticketDto = MapToDto(ticket);
            return HandleSuccess("Ticket retrieved successfully", ticketDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get tickets by client ID - Role-based access control applied
    /// </summary>
    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetTicketsByClient(int clientId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var tickets = await _ticketService.GetTicketsByClientAsync(clientId, userId.Value);
            var ticketsDto = tickets.Select(MapToDto).ToList();

            return HandleSuccess("Tickets retrieved successfully", ticketsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Get tickets by status - Role-based filtering applied
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetTicketsByStatus(string status)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var tickets = await _ticketService.GetTicketsByStatusAsync(status, userId.Value);
            var ticketsDto = tickets.Select(MapToDto).ToList();

            return HandleSuccess("Tickets retrieved successfully", ticketsDto);
        }
        catch (Exception ex)
        {
            return HandleError($"An error occurred: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Update ticket - Users can update their own tickets or staff/admin can update any
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateTicketRequest request)
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

            var ticket = await _ticketService.UpdateAsync(id, request, userId.Value);
            var ticketDto = MapToDto(ticket);

            return HandleSuccess("Ticket updated successfully", ticketDto);
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

    /// <summary>
    /// Assign ticket to an employee - Only Staff/Admin/Owner can assign
    /// </summary>
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> AssignTicket(int id, [FromBody] AssignTicketRequest request)
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

            // Only Staff/Admin/Owner can assign tickets
            if (!IsAdmin() && !IsHOD() && !IsCallingStaff())
            {
                return HandleError("Unauthorized: Only Staff, Admin, and Owner can assign tickets", 403);
            }

            var ticket = await _ticketService.AssignTicketAsync(id, request, userId.Value);
            var ticketDto = MapToDto(ticket);

            return HandleSuccess("Ticket assigned successfully", ticketDto);
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

    /// <summary>
    /// Add comment to ticket - Users with access can comment
    /// </summary>
    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddTicketCommentRequest request)
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

            var comment = await _ticketService.AddCommentAsync(id, request, userId.Value);
            var commentDto = new
            {
                comment.Id,
                comment.TicketId,
                comment.UserId,
                UserName = comment.User?.Name,
                UserEmail = comment.User?.Email,
                UserRole = comment.User?.Role?.Name,
                comment.Comment,
                comment.IsInternal,
                comment.CreatedAt,
                comment.UpdatedAt
            };

            return StatusCode(201, new { message = "Comment added successfully", data = commentDto, success = true });
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

    /// <summary>
    /// Get comments for a ticket - Role-based filtering applied
    /// </summary>
    [HttpGet("{id}/comments")]
    public async Task<IActionResult> GetComments(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            var comments = await _ticketService.GetCommentsAsync(id, userId.Value);
            var commentsDto = comments.Select(c => new
            {
                c.Id,
                c.TicketId,
                c.UserId,
                UserName = c.User?.Name,
                UserEmail = c.User?.Email,
                UserRole = c.User?.Role?.Name,
                c.Comment,
                c.IsInternal,
                c.CreatedAt,
                c.UpdatedAt
            }).ToList();

            return HandleSuccess("Comments retrieved successfully", commentsDto);
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

    /// <summary>
    /// Delete ticket (soft delete) - Users can delete their own tickets or staff/admin can delete any
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleError("Unauthorized", 401);
            }

            await _ticketService.DeleteAsync(id, userId.Value);
            return HandleSuccess("Ticket deleted successfully", null);
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

    private object MapToDto(Domain.Entities.Ticket ticket)
    {
        return new
        {
            ticket.Id,
            ticket.TicketNumber,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.CreatedBy,
            CreatorName = ticket.Creator?.Name,
            CreatorEmail = ticket.Creator?.Email,
            CreatorRole = ticket.Creator?.Role?.Name,
            ticket.AssignedTo,
            AssigneeName = ticket.Assignee?.Name,
            AssigneeEmail = ticket.Assignee?.Email,
            AssigneeRole = ticket.Assignee?.Role?.Name,
            ticket.ClientId,
            ClientName = ticket.Client?.CompanyName,
            Comments = ticket.Comments?.Select(c => new
            {
                c.Id,
                c.UserId,
                UserName = c.User?.Name,
                UserEmail = c.User?.Email,
                UserRole = c.User?.Role?.Name,
                c.Comment,
                c.IsInternal,
                c.CreatedAt,
                c.UpdatedAt
            }).ToList(),
            ticket.CreatedAt,
            ticket.UpdatedAt
        };
    }
}

