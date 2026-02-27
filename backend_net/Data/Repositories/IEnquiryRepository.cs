using backend_net.Domain.Entities;

namespace backend_net.Data.Repositories;

/// <summary>
/// Repository interface for Enquiry entity
/// Follows Repository pattern and Interface Segregation Principle
/// </summary>
public interface IEnquiryRepository
{
    System.Threading.Tasks.Task<Enquiry?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetAllAsync(CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetAllAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByClientIdAsync(int clientId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByClientIdAsync(int clientId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByStatusAsync(string status, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetFilteredByClientIdsAsync(List<int> clientIds, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetFilteredByClientIdsAsync(List<int> clientIds, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByStatusFilteredByClientIdsAsync(string status, List<int> clientIds, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IEnumerable<Enquiry>> GetByStatusFilteredByClientIdsAsync(string status, List<int> clientIds, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<int> GetCountByStatusAsync(string status, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<int> GetCountByStatusFilteredByClientIdsAsync(string status, List<int> clientIds, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<int> GetTotalCountFilteredByClientIdsAsync(List<int> clientIds, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<Enquiry> AddAsync(Enquiry enquiry, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task UpdateAsync(Enquiry enquiry, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task DeleteAsync(Enquiry enquiry, CancellationToken cancellationToken = default);
}

