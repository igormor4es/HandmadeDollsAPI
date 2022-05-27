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
    [Display(Name = "Order Received")]
    ORDER_RECEIVED = 1,
    [Display(Name = "Awaiting Payment")]
    AWAITING_PAYMENT = 2,
    [Display(Name = "Order in Separation")]
    ORDER_IN_SEPARATION = 3,
    [Display(Name = "Invoice Issued")]
    INVOICE_ISSUED = 4,
    [Display(Name = "Order Delivered")]
    ORDER_DELIVERED = 5
}

