using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Orders
{
    public class OrderItemDetail
    {
        public int? Quantity {  get; set; }
        public string? ItemName {  get; set; }
        public decimal Cost { get; set;}
        public decimal Consultation { get; set; }
        public string? ConsultationNote { get; set; }
    }
}
