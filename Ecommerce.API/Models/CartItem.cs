using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Models;

public class CartItem
{
    [Key]
    public int CartItemId { get; set; }

    [Required]
    public int CartId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public int Quantity { get; set; }

    public Cart? Cart { get; set; }

    public Product? Product { get; set; }
}