using System.ComponentModel.DataAnnotations;

namespace HandmadeDolls.Models;

public class Order
{    
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    
    public OrderStatus OrderStatus { get; set; }    

    public ICollection<OrderList>? OrderLists { get; set; }
}

public enum OrderStatus
{
    ORDER_RECEIVED = 1,
    AWAITING_PAYMENT = 2,
    ORDER_IN_SEPARATION = 3,
    INVOICE_ISSUED = 4,
    ORDER_DELIVERED = 5
}

