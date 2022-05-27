namespace HandmadeDolls.Models;

public class OrderList
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }
}