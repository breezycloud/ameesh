

using Shared.Models.Orders;

namespace Shared.Helpers;

public class OrderCartUpdateEventArgs : EventArgs
{
    public List<OrderCartRow> Bill { get; set; }
    public int Count { get; set; }

    public OrderCartUpdateEventArgs(List<OrderCartRow> bill, int count)
    {
        Bill = bill;
        Count = count;
    }
}