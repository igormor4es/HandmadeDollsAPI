namespace HandmadeDolls.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    
    public int StatusId { get; set; }
    public Status? Status { get; set; }

    public ICollection<OrderList>? OrderLists { get; set; }
}
