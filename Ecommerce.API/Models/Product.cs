using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ecommerce.API.Models.Identity;

namespace Ecommerce.API.Models;

public class Product
{
    [Key]
    public int ProductId { get; set; }

    public string? OwnerId { get; set; }

    public ApplicationUser? Owner { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int BrandId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    public int Stock { get; set; }

    public string? Image { get; set; }

    // Quan hệ với Category
    public Category? Category { get; set; }

    // Quan hệ với Brand
    public Brand? Brand { get; set; }

    // Các quan hệ khác
    public ICollection<OrderDetail> OrderDetails { get; set; }
        = new List<OrderDetail>();

    public ICollection<CartItem> CartItems { get; set; }
        = new List<CartItem>();

    public ICollection<Review> Reviews { get; set; }
        = new List<Review>();

    public ICollection<UserBehavior> UserBehaviors { get; set; }
        = new List<UserBehavior>();

    public ICollection<Recommendation> Recommendations { get; set; }
        = new List<Recommendation>();
}