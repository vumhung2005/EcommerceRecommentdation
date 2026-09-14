using System.ComponentModel.DataAnnotations;
using Ecommerce.API.Models.Identity;

namespace Ecommerce.API.Models;

public class Review
{
    [Key]
    public int ReviewId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    public string? Comment { get; set; }

    public ApplicationUser? User { get; set; }

    public Product? Product { get; set; }
}