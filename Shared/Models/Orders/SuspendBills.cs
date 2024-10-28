using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace Shared.Models.Orders;

public record SuspendBills(List<OrderCartRow>? Rows, DateTime Date);