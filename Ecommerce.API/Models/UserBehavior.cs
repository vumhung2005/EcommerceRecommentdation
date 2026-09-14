using System.ComponentModel.DataAnnotations;
using Ecommerce.API.Models.Identity;

namespace Ecommerce.API.Models;

public class UserBehavior
{
    [Key]
    public int UserBehaviorId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActionType { get; set; } = string.Empty;

    [Required]
    public int Weight { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApplicationUser? User { get; set; }

    public Product? Product { get; set; }
}