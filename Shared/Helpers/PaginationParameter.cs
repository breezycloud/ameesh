using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers;

public class PaginationParameter
{
	public string? SearchTerm { get; set; } = null;
	public string? PaymentStatus { get; set; } = null;
	public Guid? FilterId { get;set; } = null;
	public int Page { get; set; }
	public int PageSize { get; set; }	
	public bool HasThirdItems { get; set; }
}
