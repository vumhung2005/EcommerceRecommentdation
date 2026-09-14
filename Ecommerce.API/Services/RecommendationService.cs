using Ecommerce.API.Data;
using Ecommerce.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Services;

public class RecommendationService
{
    private readonly AppDbContext _context;

    public RecommendationService(AppDbContext context)
    {
        _context = context;
    }

  
    public async Task<List<Recommendation>> GenerateRecommendationsAsync(
        string userId,
        int limit = 10)
    {
        
        var behaviors = await _context.UserBehaviors
            .Where(x => x.UserId == userId)
            .ToListAsync();

        
        if (!behaviors.Any())
        {
            return await GeneratePopularRecommendationsAsync(
                userId,
                limit);
        }

        
        var productScores = behaviors
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Score = g.Sum(x => x.Weight)
            })
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .ToList();

        
        var recommendations = await SaveRecommendationsAsync(
            userId,
            productScores.Select(x => (x.ProductId, (double)x.Score)));

        var productIds = recommendations
            .Select(x => x.ProductId)
            .ToList();

        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => productIds.Contains(p.ProductId))
            .ToListAsync();

        foreach (var recommendation in recommendations)
        {
            recommendation.Product = products
                .FirstOrDefault(p =>
                    p.ProductId == recommendation.ProductId);
        }

        return recommendations
            .OrderByDescending(x => x.Score)
            .ToList();
    }


   
    private async Task<List<Recommendation>>
        GeneratePopularRecommendationsAsync(
            string userId,
            int limit)
    {
        var popularProducts = await _context.UserBehaviors
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Score = g.Sum(x => x.Weight)
            })
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .ToListAsync();

        
        if (!popularProducts.Any())
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderByDescending(p => p.ProductId)
                .Take(limit)
                .ToListAsync();

            return products.Select(p => new Recommendation
            {
                UserId = userId,
                ProductId = p.ProductId,
                Score = 0,
                CreatedAt = DateTime.UtcNow,
                Product = p
            }).ToList();
        }

        
        var recommendations = await SaveRecommendationsAsync(
            userId,
            popularProducts.Select(x => (x.ProductId, (double)x.Score)));

        var productIds = recommendations
            .Select(x => x.ProductId)
            .ToList();

        var productsResult = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => productIds.Contains(p.ProductId))
            .ToListAsync();

        foreach (var recommendation in recommendations)
        {
            recommendation.Product = productsResult
                .FirstOrDefault(p =>
                    p.ProductId == recommendation.ProductId);
        }

        return recommendations
            .OrderByDescending(x => x.Score)
            .ToList();
    }

    private async Task<List<Recommendation>> SaveRecommendationsAsync(
        string userId,
        IEnumerable<(int ProductId, double Score)> productScores)
    {
        var recommendations = new List<Recommendation>();

        foreach (var (productId, score) in productScores)
        {
            var recommendation = await _context.Recommendations
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.ProductId == productId);

            if (recommendation == null)
            {
                recommendation = new Recommendation
                {
                    UserId = userId,
                    ProductId = productId
                };
                _context.Recommendations.Add(recommendation);
            }

            recommendation.Score = score;
            recommendation.CreatedAt = DateTime.UtcNow;
            recommendations.Add(recommendation);
        }

        await _context.SaveChangesAsync();
        return recommendations;
    }


    
    public async Task<List<Recommendation>>
        GetSavedRecommendationsAsync(
            string userId,
            int limit = 10)
    {
        return await _context.Recommendations
            .Where(x => x.UserId == userId)
            .Include(x => x.Product)
                .ThenInclude(p => p!.Category)
            .Include(x => x.Product)
                .ThenInclude(p => p!.Brand)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }
}