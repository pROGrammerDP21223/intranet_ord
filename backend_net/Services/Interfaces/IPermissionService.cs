using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;

namespace backend_net.Services.Interfaces;

public interface IPermissionService
{
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<IEnumerable<object>> GetByCategoryAsync();
    Task<Permission?> GetByIdAsync(int id);
    Task<Permission> CreateAsync(CreatePermissionRequest request);
    Task<Permission> UpdateAsync(int id, UpdatePermissionRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> PermissionNameExistsAsync(string name, int? excludePermissionId = null);
}

