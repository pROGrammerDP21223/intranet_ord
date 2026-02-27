using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;

namespace backend_net.Services.Interfaces;

public interface IUserClientService
{
    System.Threading.Tasks.Task AttachUserToClientAsync(AttachUserToClientRequest request);
    System.Threading.Tasks.Task DetachUserFromClientAsync(AttachUserToClientRequest request);
    System.Threading.Tasks.Task<IEnumerable<Client>> GetUserClientsAsync(int userId);
    System.Threading.Tasks.Task<IEnumerable<User>> GetClientUsersAsync(int clientId);
    System.Threading.Tasks.Task<bool> IsUserAttachedToClientAsync(int userId, int clientId);
}

public interface ISalesPersonClientService
{
    System.Threading.Tasks.Task AttachSalesPersonToClientAsync(AttachSalesPersonToClientRequest request);
    System.Threading.Tasks.Task AttachMultipleSalesPersonsToClientAsync(AttachMultipleSalesPersonsToClientRequest request);
    System.Threading.Tasks.Task DetachSalesPersonFromClientAsync(AttachSalesPersonToClientRequest request);
    System.Threading.Tasks.Task<IEnumerable<Client>> GetSalesPersonClientsAsync(int salesPersonId);
    System.Threading.Tasks.Task<IEnumerable<User>> GetClientSalesPersonsAsync(int clientId);
    System.Threading.Tasks.Task<bool> IsSalesPersonAttachedToClientAsync(int salesPersonId, int clientId);
}

public interface ISalesManagerSalesPersonService
{
    System.Threading.Tasks.Task AttachSalesManagerToSalesPersonAsync(AttachSalesManagerToSalesPersonRequest request);
    System.Threading.Tasks.Task DetachSalesManagerFromSalesPersonAsync(AttachSalesManagerToSalesPersonRequest request);
    System.Threading.Tasks.Task<IEnumerable<User>> GetSalesManagerSalesPersonsAsync(int salesManagerId);
    System.Threading.Tasks.Task<IEnumerable<User>> GetSalesPersonManagersAsync(int salesPersonId);
    System.Threading.Tasks.Task<bool> IsSalesManagerAttachedToSalesPersonAsync(int salesManagerId, int salesPersonId);
}

public interface ISalesManagerClientService
{
    System.Threading.Tasks.Task AttachSalesManagerToClientAsync(AttachSalesManagerToClientRequest request);
    System.Threading.Tasks.Task DetachSalesManagerFromClientAsync(AttachSalesManagerToClientRequest request);
    System.Threading.Tasks.Task<IEnumerable<Client>> GetSalesManagerClientsAsync(int salesManagerId);
    System.Threading.Tasks.Task<IEnumerable<User>> GetClientSalesManagersAsync(int clientId);
    System.Threading.Tasks.Task<bool> IsSalesManagerAttachedToClientAsync(int salesManagerId, int clientId);
}

public interface IOwnerClientService
{
    System.Threading.Tasks.Task AttachOwnerToClientAsync(AttachOwnerToClientRequest request);
    System.Threading.Tasks.Task DetachOwnerFromClientAsync(AttachOwnerToClientRequest request);
    System.Threading.Tasks.Task<IEnumerable<Client>> GetOwnerClientsAsync(int ownerId);
    System.Threading.Tasks.Task<IEnumerable<User>> GetClientOwnersAsync(int clientId);
    System.Threading.Tasks.Task<bool> IsOwnerAttachedToClientAsync(int ownerId, int clientId);
}

