using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers;

public class GridDataRequest
{	
	public int Page { get; set; } = 0; // The page number for the data we're requesting
	public int PageSize { get; set; } = 10; // The number of items per page
}

public class GridDataResponse<T>
{
	public List<T>? Data { get; set; }
	public int TotalCount { get; set; }
}
