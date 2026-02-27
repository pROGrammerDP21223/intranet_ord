using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;

namespace backend_net.Services.Interfaces;

public interface IServiceService
{
    System.Threading.Tasks.Task<IEnumerable<Service>> GetAllAsync(bool includeInactive = false);
    System.Threading.Tasks.Task<IEnumerable<object>> GetByCategoryAsync();
    System.Threading.Tasks.Task<Service?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<Service> CreateAsync(CreateServiceRequest request);
    System.Threading.Tasks.Task<Service> UpdateAsync(int id, UpdateServiceRequest request);
    System.Threading.Tasks.Task DeleteAsync(int id);
}

