using backend_net.Data.Context;
using backend_net.DTOs.Requests;
using backend_net.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Services;

public class ClientProductService : IClientProductService
{
    private readonly ApplicationDbContext _context;

    public ClientProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task AttachProductToClientAsync(AttachProductToClientRequest request)
    {
        // Verify client exists
        var client = await _context.Clients.FindAsync(request.ClientId);
        if (client == null || client.IsDeleted)
        {
            throw new KeyNotFoundException("Client not found");
        }

        // Verify product exists
        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null || product.IsDeleted)
        {
            throw new KeyNotFoundException("Product not found");
        }

        // Check if already attached
        var existing = await _context.ClientProducts
            .FirstOrDefaultAsync(cp => cp.ClientId == request.ClientId 
                && cp.ProductId == request.ProductId 
                && !cp.IsDeleted);

        if (existing != null)
        {
            throw new InvalidOperationException("Product is already attached to this client");
        }

        var clientProduct = new Domain.Entities.ClientProduct
        {
            ClientId = request.ClientId,
            ProductId = request.ProductId
        };

        await _context.ClientProducts.AddAsync(clientProduct);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task AttachMultipleProductsToClientAsync(AttachMultipleProductsToClientRequest request)
    {
        // Verify client exists
        var client = await _context.Clients.FindAsync(request.ClientId);
        if (client == null || client.IsDeleted)
        {
            throw new KeyNotFoundException("Client not found");
        }

        // Verify all products exist and are not deleted
        var productIds = request.ProductIds.Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync();

        if (products.Count != productIds.Count)
        {
            var foundIds = products.Select(p => p.Id).ToList();
            var missingIds = productIds.Except(foundIds).ToList();
            throw new KeyNotFoundException($"One or more products not found. Missing product IDs: {string.Join(", ", missingIds)}");
        }

        // Get existing attachments to avoid duplicates
        var existingAttachments = await _context.ClientProducts
            .Where(cp => cp.ClientId == request.ClientId 
                && productIds.Contains(cp.ProductId) 
                && !cp.IsDeleted)
            .Select(cp => cp.ProductId)
            .ToListAsync();

        // Filter out already attached products
        var productsToAttach = productIds.Except(existingAttachments).ToList();

        if (productsToAttach.Count == 0)
        {
            throw new InvalidOperationException("All selected products are already attached to this client");
        }

        // Create new attachments
        var clientProducts = productsToAttach.Select(productId => new Domain.Entities.ClientProduct
        {
            ClientId = request.ClientId,
            ProductId = productId
        }).ToList();

        await _context.ClientProducts.AddRangeAsync(clientProducts);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DetachProductFromClientAsync(DetachProductFromClientRequest request)
    {
        var clientProduct = await _context.ClientProducts
            .FirstOrDefaultAsync(cp => cp.ClientId == request.ClientId 
                && cp.ProductId == request.ProductId 
                && !cp.IsDeleted);

        if (clientProduct == null)
        {
            throw new KeyNotFoundException("Product is not attached to this client");
        }

        // Soft delete
        clientProduct.IsDeleted = true;
        clientProduct.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<Domain.Entities.Product>> GetClientProductsAsync(int clientId)
    {
        return await _context.ClientProducts
            .Include(cp => cp.Product)
                .ThenInclude(p => p!.Category)
                    .ThenInclude(c => c!.Industry)
            .Include(cp => cp.Product)
                .ThenInclude(p => p!.ProductImages)
            .Where(cp => cp.ClientId == clientId && !cp.IsDeleted && cp.Product != null && !cp.Product.IsDeleted)
            .Select(cp => cp.Product!)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<bool> IsProductAttachedToClientAsync(int clientId, int productId)
    {
        return await _context.ClientProducts
            .AnyAsync(cp => cp.ClientId == clientId 
                && cp.ProductId == productId 
                && !cp.IsDeleted);
    }
}

