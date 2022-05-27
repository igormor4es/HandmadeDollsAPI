namespace HandmadeDolls.Models;

public class Product
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public byte[]? Image { get; set; }
    public bool Active { get; set; }
    public ProductType ProductType { get; set; }

    public ICollection<OrderList>? OrderLists { get; set; }
    public ICollection<DollAcessory>? Dolls { get; set; }
    public ICollection<DollAcessory>? Acessories { get; set; }
}
public enum ProductType
{
    DOLL = 1, 
    ACESSORY = 2
}
