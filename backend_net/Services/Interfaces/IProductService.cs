using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;

namespace backend_net.Services.Interfaces;

public interface IProductService
{
    System.Threading.Tasks.Task<IEnumerable<Product>> GetAllAsync();
    System.Threading.Tasks.Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
    System.Threading.Tasks.Task<Product?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<Product> CreateAsync(CreateProductRequest request);
    System.Threading.Tasks.Task<Product> UpdateAsync(int id, UpdateProductRequest request);
    System.Threading.Tasks.Task DeleteAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<Product>> GetProductsByClientIdAsync(int clientId);
}

