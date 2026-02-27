using backend_net.Domain.Entities;
using backend_net.DTOs.Requests;

namespace backend_net.Services.Interfaces;

public interface IEnquiryService
{
    Task<Enquiry> CreateAsync(CreateEnquiryRequest request);
    Task<Enquiry?> GetByIdAsync(int id);
    Task<IEnumerable<Enquiry>> GetAllAsync();
    Task<IEnumerable<Enquiry>> GetAllAsync(DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<Enquiry>> GetFilteredByClientIdsAsync(List<int> clientIds);
    Task<IEnumerable<Enquiry>> GetFilteredByClientIdsAsync(List<int> clientIds, DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<Enquiry>> GetByStatusAsync(string status);
    Task<IEnumerable<Enquiry>> GetByStatusAsync(string status, DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<Enquiry>> GetByStatusFilteredByClientIdsAsync(string status, List<int> clientIds);
    Task<IEnumerable<Enquiry>> GetByStatusFilteredByClientIdsAsync(string status, List<int> clientIds, DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<Enquiry>> GetByClientIdAsync(int clientId);
    Task<IEnumerable<Enquiry>> GetByClientIdAsync(int clientId, DateTime? startDate, DateTime? endDate);
    Task<Enquiry> UpdateAsync(int id, UpdateEnquiryRequest request);
    Task<bool> DeleteAsync(int id);
    Task<int> GetCountByStatusAsync(string status);
    Task<int> GetCountByStatusFilteredByClientIdsAsync(string status, List<int> clientIds);
    Task<int> GetTotalCountAsync();
    Task<int> GetTotalCountFilteredByClientIdsAsync(List<int> clientIds);
}

