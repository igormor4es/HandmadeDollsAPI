namespace HandmadeDolls.Models;

public abstract class Product
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public byte[]? Image { get; set; }
    public bool Active { get; set; }
    
    public ICollection<OrderList>? OrderLists { get; set; }    
}

public class Doll : Product
{
    public bool IsDoll { get; set; }
    public ICollection<DollAcessory>? DollsAccessories { get; set; }
}

public class Accessory : Product
{
    public bool IsAccessory { get; set; }
    public bool IsGift { get; set; }
    public ICollection<DollAcessory>? DollsAccessories { get; set; }
}
