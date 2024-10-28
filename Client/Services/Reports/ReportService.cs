using Client.Pages.Reports;
using Client.Pages.Reports.Templates.Receipt;
using Client.Pages.Reports.Templates.Sales;
using Microsoft.JSInterop;
using QuestPDF.Fluent;
using Shared.Helpers;
using Shared.Models.Orders;
using Shared.Models.Reports;
using System.Net.Http.Json;

namespace Client.Services.Reports;

public class ReportService(IHttpClientFactory _client)
{
}
