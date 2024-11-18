
using Shared.Models.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Orders;

public class QuantityAndPrescription
{
    [Required]
    public decimal? Quantity { get; set; }
    public string? Prescription { get; set; }
}