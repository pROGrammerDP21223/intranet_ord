using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;
using System.Threading.Tasks;

namespace backend_net.Services.Interfaces;

public interface IClientService
{
    System.Threading.Tasks.Task<Client?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<Client>> GetAllAsync();
    System.Threading.Tasks.Task<Client> CreateAsync(CreateClientRequest request, int? createdByUserId = null, string? createdByRole = null);
    System.Threading.Tasks.Task<Client> UpdateAsync(int id, CreateClientRequest request);
    System.Threading.Tasks.Task<Client> ApproveClientAsync(int id, int approvedByUserId);
    System.Threading.Tasks.Task DeleteAsync(int id);
    System.Threading.Tasks.Task<bool> CustomerNoExistsAsync(string customerNo, int? excludeId = null);
}

