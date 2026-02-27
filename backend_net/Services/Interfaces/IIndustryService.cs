using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;

namespace backend_net.Services.Interfaces;

public interface IIndustryService
{
    System.Threading.Tasks.Task<IEnumerable<Industry>> GetAllAsync();
    System.Threading.Tasks.Task<Industry?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<Industry> CreateAsync(CreateIndustryRequest request);
    System.Threading.Tasks.Task<Industry> UpdateAsync(int id, UpdateIndustryRequest request);
    System.Threading.Tasks.Task DeleteAsync(int id);
}

