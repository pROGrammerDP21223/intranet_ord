using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;

namespace backend_net.Services.Interfaces;

public interface ICategoryService
{
    System.Threading.Tasks.Task<IEnumerable<Category>> GetAllAsync();
    System.Threading.Tasks.Task<IEnumerable<Category>> GetByIndustryIdAsync(int industryId);
    System.Threading.Tasks.Task<Category?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<Category> CreateAsync(CreateCategoryRequest request);
    System.Threading.Tasks.Task<Category> UpdateAsync(int id, UpdateCategoryRequest request);
    System.Threading.Tasks.Task DeleteAsync(int id);
}

