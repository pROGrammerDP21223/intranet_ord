using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;

namespace backend_net.Services.Interfaces;

public interface IClientProductService
{
    System.Threading.Tasks.Task AttachProductToClientAsync(AttachProductToClientRequest request);
    System.Threading.Tasks.Task AttachMultipleProductsToClientAsync(AttachMultipleProductsToClientRequest request);
    System.Threading.Tasks.Task DetachProductFromClientAsync(DetachProductFromClientRequest request);
    System.Threading.Tasks.Task<IEnumerable<Product>> GetClientProductsAsync(int clientId);
    System.Threading.Tasks.Task<bool> IsProductAttachedToClientAsync(int clientId, int productId);
}

