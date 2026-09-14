using Microsoft.AspNetCore.Identity;

namespace Ecommerce.API.Models.Identity;
public class ApplicationUser : IdentityUser
{
    // Add any additional properties you want for your application user here
     public string FullName { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string SellerStatus { get; set; } = "None";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Quan hệ với các bảng nghiệp vụ
    public ICollection<Order> Orders { get; set; }
    = new List<Order>();

    public ICollection<Cart> Carts { get; set; }
        = new List<Cart>();

    public ICollection<Review> Reviews { get; set; }
        = new List<Review>();

    public ICollection<UserBehavior> UserBehaviors { get; set; }
        = new List<UserBehavior>();

    public ICollection<Recommendation> Recommendations { get; set; }
        = new List<Recommendation>();
}