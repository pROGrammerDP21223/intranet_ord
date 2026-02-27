using backend_net.Domain.Entities;

namespace backend_net.Services.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int id);
    Task<IEnumerable<Permission>> GetAllPermissionsAsync();
    Task<Role> CreateAsync(string name, string? description);
    Task<Role> UpdateAsync(int id, string name, string? description);
    Task<bool> DeleteAsync(int id);
    Task<Role> AssignPermissionsAsync(int roleId, List<int> permissionIds);
}

