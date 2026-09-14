using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Models;

public class Brand
{
    [Key]
    public int BrandId { get; set; }

    [Required]
    [MaxLength(200)]
    public string BrandName { get; set; } = string.Empty;

    // 1 Brand có nhiều Product
    public ICollection<Product> Products { get; set; } = new List<Product>();
}