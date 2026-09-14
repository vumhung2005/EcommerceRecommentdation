using System.ComponentModel.DataAnnotations;
using Ecommerce.API.Models.Identity;

namespace Ecommerce.API.Models;

public class Cart
{
    [Key]
    public int CartId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}