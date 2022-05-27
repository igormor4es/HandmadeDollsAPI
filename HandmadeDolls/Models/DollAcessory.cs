namespace HandmadeDolls.Models;

public class DollAcessory
{
    public int Id { get; set; }
    public int DollId { get; set; }
    public Doll? Doll { get; set; }
    public int AccessoryId { get; set; }
    public Accessory? Accessory { get; set; }
}
