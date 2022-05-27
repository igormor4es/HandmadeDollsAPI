using System.ComponentModel.DataAnnotations;

namespace HandmadeDolls.Models;

public class Status
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public ICollection<Order>? Orders { get; set; }
}

public enum OrderStatus
{
    [Display(Name = "Order Received")]
    OrderReceived = 1,
    [Display(Name = "Awaiting Payment")]
    AwaitingPayment = 2,
    [Display(Name = "Order in Separation")]
    OrderInSeparation = 3,
    [Display(Name = "Invoice Issued")]
    InvoiceIssued = 4,
    [Display(Name = "Delivered")]
    OrderDelivered = 5
}
