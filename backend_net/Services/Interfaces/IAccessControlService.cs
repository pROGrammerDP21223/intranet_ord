namespace backend_net.Services.Interfaces;

public interface IAccessControlService
{
    Task<bool> CanUserAccessClientAsync(int userId, int clientId);
    Task<List<int>> GetAccessibleClientIdsAsync(int userId);
    Task<bool> CanUserAttachProductToClientAsync(int userId, int clientId);
}

