using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ecommerce.API.Models.Identity;

namespace Ecommerce.API.Models;

public class Order
{
    [Key]
    public int OrderId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(200)]
    public string ReceiverName { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string PaymentMethod { get; set; } = "COD";

    // FK User
    public ApplicationUser? User { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public Payment? Payment { get; set; }
}