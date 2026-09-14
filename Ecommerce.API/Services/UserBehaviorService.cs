using Ecommerce.API.Data;
using Ecommerce.API.Models;

namespace Ecommerce.API.Services;

public class UserBehaviorService
{
    private readonly AppDbContext _context;
    private readonly CollaborativeFilteringService _collaborativeService;

    public UserBehaviorService(
        AppDbContext context,
        CollaborativeFilteringService collaborativeService)
    {
        _context = context;
        _collaborativeService = collaborativeService;
    }

    public async Task AddBehaviorAsync(
        string userId,
        int productId,
        string actionType)
    {
        int weight = actionType switch
        {
            "View" => 1,
            "Click" => 2,
            "AddToCart" => 3,
            "Purchase" => 5,
            _ => 0
        };

        if (weight <= 0)
        {
            return;
        }

        var behavior = new UserBehavior
        {
            UserId = userId,
            ProductId = productId,
            ActionType = actionType,
            Weight = weight,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserBehaviors.Add(behavior);

        await _context.SaveChangesAsync();

        await _collaborativeService.SaveRecommendationsAsync(
            userId,
            topUsers: 5,
            topProducts: 10);
    }
}