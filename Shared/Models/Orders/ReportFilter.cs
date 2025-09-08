using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace Shared.Models.Orders;

public class ReportFilter
{
    [Required]
    public string? Option { get; set; } = "All";
    [Required]
    public string? Type { get; set; } = "Store";
    [Required]
    public string? Criteria { get; set; } = "Date";
    public bool IncludeThirdParty { get; set; }
    public string? ReportOption { get; set; }
    public Guid StoreID { get; set; }
    public Guid UserID { get; set; }
    public DateTime? StartDate { get; set; }   
    public DateTime? EndDate { get; set; }
}

public class ExportFilter 
{
    [Required]
    public string? Criteria { get; set; } = "Range";
    public string? Option { get; set; } = "Item";
    public Guid StoreID { get; set; }
    public DateTime? StartDate { get; set; }   
    public DateTime? EndDate { get; set; }    
}
