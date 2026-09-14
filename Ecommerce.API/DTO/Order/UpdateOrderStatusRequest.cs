using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.DTO.Order;

public class UpdateOrderStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}