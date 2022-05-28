using System.ComponentModel.DataAnnotations.Schema;

namespace HandmadeDolls.Models;

public class DollAcessory
{
    public int Id { get; set; }
    public int DollId { get; set; }
    public Product? ProductDoll { get; set; }
    public int AccessoryId { get; set; }
    public Product? ProductAcessory { get; set; }
    [NotMapped]
    public string? Description { get; set; }
}
