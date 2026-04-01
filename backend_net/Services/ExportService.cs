using System.Text;
using backend_net.Domain.Entities;
using backend_net.Services.Interfaces;
using ClosedXML.Excel;
using Task = System.Threading.Tasks.Task;

namespace backend_net.Services;

public class ExportService : IExportService
{
    public Task<byte[]> ExportClientsToExcelAsync(IEnumerable<Client> clients)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Clients");

        worksheet.Cell(1, 1).Value = "Customer No";
        worksheet.Cell(1, 2).Value = "Company Name";
        worksheet.Cell(1, 3).Value = "Contact Person";
        worksheet.Cell(1, 4).Value = "Email";
        worksheet.Cell(1, 5).Value = "Phone";
        worksheet.Cell(1, 6).Value = "Status";
        worksheet.Cell(1, 7).Value = "Amount Without GST";
        worksheet.Cell(1, 8).Value = "Created At";

        var row = 2;
        foreach (var client in clients)
        {
            worksheet.Cell(row, 1).Value = client.CustomerNo;
            worksheet.Cell(row, 2).Value = client.CompanyName;
            worksheet.Cell(row, 3).Value = client.ContactPerson;
            worksheet.Cell(row, 4).Value = client.Email ?? string.Empty;
            worksheet.Cell(row, 5).Value = client.Phone ?? client.WhatsAppNumber ?? string.Empty;
            worksheet.Cell(row, 6).Value = client.Status;
            worksheet.Cell(row, 7).Value = client.AmountWithoutGst ?? 0m;
            worksheet.Cell(row, 8).Value = client.CreatedAt;
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    public Task<byte[]> ExportClientsToCsvAsync(IEnumerable<Client> clients)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Customer No,Company Name,Contact Person,Email,Phone,Status,Amount Without GST,Created At");

        foreach (var client in clients)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsvField(client.CustomerNo),
                EscapeCsvField(client.CompanyName),
                EscapeCsvField(client.ContactPerson),
                EscapeCsvField(client.Email),
                EscapeCsvField(client.Phone ?? client.WhatsAppNumber),
                EscapeCsvField(client.Status),
                EscapeCsvField(client.AmountWithoutGst?.ToString() ?? "0"),
                EscapeCsvField(client.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
            ));
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(csv.ToString()));
    }

    public Task<byte[]> ExportEnquiriesToExcelAsync(IEnumerable<Enquiry> enquiries)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Enquiries");

        worksheet.Cell(1, 1).Value = "Full Name";
        worksheet.Cell(1, 2).Value = "Email";
        worksheet.Cell(1, 3).Value = "Mobile";
        worksheet.Cell(1, 4).Value = "Client";
        worksheet.Cell(1, 5).Value = "Status";
        worksheet.Cell(1, 6).Value = "Source";
        worksheet.Cell(1, 7).Value = "Created At";
        worksheet.Cell(1, 8).Value = "Notes";

        var row = 2;
        foreach (var enquiry in enquiries)
        {
            worksheet.Cell(row, 1).Value = enquiry.FullName;
            worksheet.Cell(row, 2).Value = enquiry.EmailId;
            worksheet.Cell(row, 3).Value = enquiry.MobileNumber;
            worksheet.Cell(row, 4).Value = enquiry.Client?.CompanyName ?? string.Empty;
            worksheet.Cell(row, 5).Value = enquiry.Status;
            worksheet.Cell(row, 6).Value = enquiry.Source ?? string.Empty;
            worksheet.Cell(row, 7).Value = enquiry.CreatedAt;
            worksheet.Cell(row, 8).Value = enquiry.Notes ?? string.Empty;
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    public Task<byte[]> ExportEnquiriesToCsvAsync(IEnumerable<Enquiry> enquiries)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Full Name,Email,Mobile,Client,Status,Source,Created At,Notes");

        foreach (var enquiry in enquiries)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsvField(enquiry.FullName),
                EscapeCsvField(enquiry.EmailId),
                EscapeCsvField(enquiry.MobileNumber),
                EscapeCsvField(enquiry.Client?.CompanyName),
                EscapeCsvField(enquiry.Status),
                EscapeCsvField(enquiry.Source),
                EscapeCsvField(enquiry.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")),
                EscapeCsvField(enquiry.Notes)
            ));
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(csv.ToString()));
    }

    public Task<byte[]> ExportTransactionsToExcelAsync(IEnumerable<Transaction> transactions)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Transactions");

        worksheet.Cell(1, 1).Value = "Transaction No";
        worksheet.Cell(1, 2).Value = "Type";
        worksheet.Cell(1, 3).Value = "Amount";
        worksheet.Cell(1, 4).Value = "Date";
        worksheet.Cell(1, 5).Value = "Client";
        worksheet.Cell(1, 6).Value = "Payment Method";
        worksheet.Cell(1, 7).Value = "Reference";
        worksheet.Cell(1, 8).Value = "Notes";

        var row = 2;
        foreach (var transaction in transactions)
        {
            worksheet.Cell(row, 1).Value = transaction.TransactionNumber;
            worksheet.Cell(row, 2).Value = transaction.TransactionType;
            worksheet.Cell(row, 3).Value = transaction.Amount;
            worksheet.Cell(row, 4).Value = transaction.TransactionDate;
            worksheet.Cell(row, 5).Value = transaction.Client?.CompanyName ?? string.Empty;
            worksheet.Cell(row, 6).Value = transaction.PaymentMethod ?? string.Empty;
            worksheet.Cell(row, 7).Value = transaction.ReferenceNumber ?? string.Empty;
            worksheet.Cell(row, 8).Value = transaction.Notes ?? string.Empty;
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    public Task<byte[]> ExportTransactionsToCsvAsync(IEnumerable<Transaction> transactions)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Transaction No,Type,Amount,Date,Client,Payment Method,Reference,Notes");

        foreach (var transaction in transactions)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsvField(transaction.TransactionNumber),
                EscapeCsvField(transaction.TransactionType),
                EscapeCsvField(transaction.Amount.ToString("0.00")),
                EscapeCsvField(transaction.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss")),
                EscapeCsvField(transaction.Client?.CompanyName),
                EscapeCsvField(transaction.PaymentMethod),
                EscapeCsvField(transaction.ReferenceNumber),
                EscapeCsvField(transaction.Notes)
            ));
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(csv.ToString()));
    }

    private static string EscapeCsvField(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        var escaped = value.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }
}
