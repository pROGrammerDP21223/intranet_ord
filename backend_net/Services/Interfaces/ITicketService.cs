using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;

namespace backend_net.Services.Interfaces;

public interface ITicketService
{
    System.Threading.Tasks.Task<Ticket?> GetByIdAsync(int id, int? userId = null);
    System.Threading.Tasks.Task<IEnumerable<Ticket>> GetAllAsync(int? userId = null);
    System.Threading.Tasks.Task<IEnumerable<Ticket>> GetTicketsByClientAsync(int clientId, int? userId = null);
    System.Threading.Tasks.Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(string status, int? userId = null);
    System.Threading.Tasks.Task<Ticket> CreateAsync(CreateTicketRequest request, int createdByUserId);
    System.Threading.Tasks.Task<Ticket> UpdateAsync(int id, UpdateTicketRequest request, int? userId = null);
    System.Threading.Tasks.Task<Ticket> AssignTicketAsync(int ticketId, AssignTicketRequest request, int assignedByUserId);
    System.Threading.Tasks.Task<TicketComment> AddCommentAsync(int ticketId, AddTicketCommentRequest request, int userId);
    System.Threading.Tasks.Task<IEnumerable<TicketComment>> GetCommentsAsync(int ticketId, int? userId = null);
    System.Threading.Tasks.Task<bool> CanUserAccessTicketAsync(int ticketId, int userId);
    System.Threading.Tasks.Task DeleteAsync(int id, int? userId = null);
}

