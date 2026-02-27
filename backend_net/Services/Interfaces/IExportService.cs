using backend_net.Domain.Entities;

namespace backend_net.Services.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportClientsToExcelAsync(IEnumerable<Client> clients);
    Task<byte[]> ExportClientsToCsvAsync(IEnumerable<Client> clients);
    Task<byte[]> ExportEnquiriesToExcelAsync(IEnumerable<Enquiry> enquiries);
    Task<byte[]> ExportEnquiriesToCsvAsync(IEnumerable<Enquiry> enquiries);
    Task<byte[]> ExportTransactionsToExcelAsync(IEnumerable<Transaction> transactions);
    Task<byte[]> ExportTransactionsToCsvAsync(IEnumerable<Transaction> transactions);
}
