using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ecommerce.API.Models.Identity;

namespace Ecommerce.API.Models;

public class Recommendation
{
    [Key]
    public int RecommendationId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int ProductId { get; set; }

    [Required]
    [Column(TypeName = "float")]
    public double Score { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }

    public Product? Product { get; set; }
}