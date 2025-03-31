using nhom5_webAPI.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class PetSupplyCategory
{
    [Key]
    public int SupplyCategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [JsonIgnore] // Ngăn không serialize navigation property
    public ICollection<Product>? Products { get; set; }
}
