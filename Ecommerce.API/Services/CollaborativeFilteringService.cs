using Ecommerce.API.Data;
using Ecommerce.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Services;

public class CollaborativeFilteringService
{
    private readonly AppDbContext _context;

    public CollaborativeFilteringService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProductMatrixDemoResult>
        BuildDemoMatrixAsync()
    {
        var behaviors = await _context.UserBehaviors
            .AsNoTracking()
            .OrderBy(x => x.UserId)
            .ThenBy(x => x.ProductId)
            .ToListAsync();

        var users = behaviors
            .Select(x => x.UserId)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var products = behaviors
            .Select(x => x.ProductId)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var matrix = new Dictionary<string, Dictionary<int, double>>();

        foreach (var userId in users)
        {
            var row = new Dictionary<int, double>();

            foreach (var productId in products)
            {
                row[productId] = 0;
            }

            var grouped = behaviors
                .Where(x => x.UserId == userId)
                .GroupBy(x => x.ProductId);

            foreach (var group in grouped)
            {
                row[group.Key] = group.Sum(x => x.Weight);
            }

            matrix[userId] = row;
        }

        var similarityMatrix = new Dictionary<string, Dictionary<string, double>>();

        foreach (var userA in users)
        {
            similarityMatrix[userA] = new Dictionary<string, double>();

            foreach (var userB in users)
            {
                if (userA == userB)
                {
                    similarityMatrix[userA][userB] = 1.0;
                    continue;
                }

                var similarity = CalculateCosineSimilarity(
                    matrix[userA],
                    matrix[userB]);

                similarityMatrix[userA][userB] = Math.Round(similarity, 4);
            }
        }

        var recommendations = new Dictionary<string, List<RecommendationResult>>();

        foreach (var userId in users)
        {
            var currentUserVector = matrix[userId];
            var similarUsers = new List<SimilarUserResult>();

            foreach (var otherUser in users)
            {
                if (otherUser == userId)
                {
                    continue;
                }

                var similarity = similarityMatrix[userId][otherUser];

                if (similarity <= 0)
                {
                    continue;
                }

                similarUsers.Add(new SimilarUserResult
                {
                    UserId = otherUser,
                    Similarity = similarity
                });
            }

            similarUsers = similarUsers
                .OrderByDescending(x => x.Similarity)
                .ToList();

            var productScores = new Dictionary<int, double>();

            foreach (var similarUser in similarUsers)
            {
                var similarUserVector = matrix[similarUser.UserId];

                foreach (var product in similarUserVector)
                {
                    if (currentUserVector.TryGetValue(
                            product.Key,
                            out var currentUserWeight) &&
                        currentUserWeight > 0)
                    {
                        continue;
                    }

                    if (product.Value <= 0)
                    {
                        continue;
                    }

                    var score = product.Value * similarUser.Similarity;

                    productScores[product.Key] =
                        productScores.ContainsKey(product.Key)
                            ? productScores[product.Key] + score
                            : score;
                }
            }

            recommendations[userId] = productScores
                .OrderByDescending(x => x.Value)
                .Select(x => new RecommendationResult
                {
                    ProductId = x.Key,
                    Score = Math.Round(x.Value, 4)
                })
                .ToList();
        }

        return new UserProductMatrixDemoResult
        {
            Users = users,
            Products = products,
            Matrix = matrix,
            SimilarityMatrix = similarityMatrix,
            Recommendations = recommendations,
            TotalUsers = users.Count,
            TotalProducts = products.Count
        };
    }

    public async Task<Dictionary<string, Dictionary<int, double>>>
        BuildUserProductMatrixAsync()
    {
        var demo = await BuildDemoMatrixAsync();
        return demo.Matrix;
    }


    public double CalculateCosineSimilarity(
        Dictionary<int, double> userA,
        Dictionary<int, double> userB)
    {
        var allProducts = userA.Keys
            .Union(userB.Keys)
            .ToList();

        double dotProduct = 0;

        double magnitudeA = 0;

        double magnitudeB = 0;

        foreach (var productId in allProducts)
        {
            double valueA =
                userA.ContainsKey(productId)
                    ? userA[productId]
                    : 0;

            double valueB =
                userB.ContainsKey(productId)
                    ? userB[productId]
                    : 0;

            
            dotProduct += valueA * valueB;

            
            magnitudeA += valueA * valueA;

            
            magnitudeB += valueB * valueB;
        }

        
        if (magnitudeA == 0 ||
            magnitudeB == 0)
        {
            return 0;
        }

        return dotProduct /
               (
                   Math.Sqrt(magnitudeA) *
                   Math.Sqrt(magnitudeB)
               );
    }


    

    public async Task<List<SimilarUserResult>>
        GetSimilarUsersAsync(
            string userId,
            int topN = 5)
    {
        var matrix =
            await BuildUserProductMatrixAsync();

       
        if (!matrix.ContainsKey(userId))
        {
            return new List<SimilarUserResult>();
        }

        var currentUserVector =
            matrix[userId];

        var results =
            new List<SimilarUserResult>();

        foreach (var user in matrix)
        {
            
            if (user.Key == userId)
            {
                continue;
            }

            double similarity =
                CalculateCosineSimilarity(
                    currentUserVector,
                    user.Value
                );

            results.Add(
                new SimilarUserResult
                {
                    UserId = user.Key,
                    Similarity = similarity
                }
            );
        }

        return results
            .OrderByDescending(x => x.Similarity)
            .Take(topN)
            .ToList();
    }


  

    public async Task<Dictionary<int, double>>
        GetUserVectorAsync(string userId)
    {
        var matrix =
            await BuildUserProductMatrixAsync();

        if (!matrix.ContainsKey(userId))
        {
            return new Dictionary<int, double>();
        }

        return matrix[userId];
    }


   

    public async Task<List<RecommendationResult>>
        GenerateRecommendationsAsync(
            string userId,
            int topUsers = 5,
            int topProducts = 10)
    {
        var matrix =
            await BuildUserProductMatrixAsync();

        
        if (!matrix.ContainsKey(userId))
        {
            return new List<RecommendationResult>();
        }

        var currentUserVector =
            matrix[userId];

       

        var similarUsers =
            await GetSimilarUsersAsync(
                userId,
                topUsers);

       

        var productScores =
            new Dictionary<int, double>();

        foreach (var similarUser in similarUsers)
        {
            if (!matrix.ContainsKey(
                    similarUser.UserId))
            {
                continue;
            }

            var similarUserVector =
                matrix[similarUser.UserId];

            foreach (var product in similarUserVector)
            {
                int productId =
                    product.Key;

                double behaviorWeight =
                    product.Value;

                if (currentUserVector.TryGetValue(
                        productId,
                        out var currentUserWeight) &&
                    currentUserWeight > 0)
                {
                    continue;
                }

                double score =
                    similarUser.Similarity *
                    behaviorWeight;

                if (productScores
                    .ContainsKey(productId))
                {
                    productScores[productId]
                        += score;
                }
                else
                {
                    productScores[productId]
                        = score;
                }
            }
        }

        if (productScores.Count == 0)
        {
            foreach (var otherUser in matrix)
            {
                if (otherUser.Key == userId)
                {
                    continue;
                }

                foreach (var product in otherUser.Value)
                {
                    if (currentUserVector.TryGetValue(
                            product.Key,
                            out var currentUserWeight) &&
                        currentUserWeight > 0)
                    {
                        continue;
                    }

                    if (product.Value <= 0)
                    {
                        continue;
                    }

                    productScores[product.Key] =
                        productScores.ContainsKey(product.Key)
                            ? productScores[product.Key] + product.Value
                            : product.Value;
                }
            }
        }

        var collaborativeRecommendations = productScores
            .Where(x => x.Value > 0)
            .OrderByDescending(x => x.Value)
            .Select(x =>
                new RecommendationResult
                {
                    ProductId = x.Key,
                    Score = x.Value
                })
            .ToList();

        var selectedRecommendations = collaborativeRecommendations
            .Take(topProducts)
            .ToList();

        return selectedRecommendations;
    }


    

    public async Task<int>
        SaveRecommendationsAsync(
            string userId,
            int topUsers = 5,
            int topProducts = 10)
    {
        var recommendations =
            await GenerateRecommendationsAsync(
                userId,
                topUsers,
                topProducts);

        var recommendationProductIds = recommendations
            .Select(x => x.ProductId)
            .ToHashSet();

        var staleRecommendations = await _context.Recommendations
            .Where(x =>
                x.UserId == userId &&
                !recommendationProductIds.Contains(x.ProductId))
            .ToListAsync();

        _context.Recommendations.RemoveRange(staleRecommendations);

        if (recommendations.Count == 0)
        {
            await _context.SaveChangesAsync();
            return 0;
        }

        

        foreach (var recommendation
                 in recommendations)
        {
            var savedRecommendation =
                await _context.Recommendations
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId &&
                        x.ProductId == recommendation.ProductId);

            if (savedRecommendation == null)
            {
                _context.Recommendations.Add(
                    new Recommendation
                    {
                        UserId = userId,
                        ProductId = recommendation.ProductId,
                        Score = recommendation.Score,
                        CreatedAt = DateTime.UtcNow
                    });
            }
            else
            {
                savedRecommendation.Score = recommendation.Score;
                savedRecommendation.CreatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return recommendations.Count;
    }


    

    public async Task<List<RecommendationResult>>
        GetSavedRecommendationsAsync(
            string userId,
            int topProducts = 10)
    {
        var recommendations =
            await _context.Recommendations
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.CreatedAt)
                .Take(topProducts)
                .ToListAsync();

        return recommendations
            .Select(x =>
                new RecommendationResult
                {
                    ProductId =
                        x.ProductId,

                    Score =
                        x.Score
                })
            .ToList();
    }

    public async Task<List<RecommendationWithProductResult>>
        GetSavedRecommendationsWithProductsAsync(
            string userId,
            int topProducts = 10)
    {
        return await _context.Recommendations
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.CreatedAt)
            .Take(topProducts)
            .Select(x => new RecommendationWithProductResult
            {
                ProductId = x.ProductId,
                Score = x.Score,
                Product = x.Product == null
                    ? null
                    : new RecommendedProductResult
                    {
                        ProductId = x.Product.ProductId,
                        CategoryId = x.Product.CategoryId,
                        BrandId = x.Product.BrandId,
                        Name = x.Product.Name,
                        Description = x.Product.Description,
                        Price = x.Product.Price,
                        Stock = x.Product.Stock,
                        Image = x.Product.Image
                    }
            })
            .ToListAsync();
    }
}




public class SimilarUserResult
{
    public string UserId { get; set; }
        = string.Empty;

    public double Similarity { get; set; }
}




public class RecommendationResult
{
    public int ProductId { get; set; }

    public double Score { get; set; }
}

public class RecommendationWithProductResult : RecommendationResult
{
    public RecommendedProductResult? Product { get; set; }
}

public class RecommendedProductResult
{
    public int ProductId { get; set; }

    public int CategoryId { get; set; }

    public int BrandId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string? Image { get; set; }
}

public class UserProductMatrixDemoResult
{
    public List<string> Users { get; set; } = new();

    public List<int> Products { get; set; } = new();

    public Dictionary<string, Dictionary<int, double>> Matrix { get; set; } = new();

    public Dictionary<string, Dictionary<string, double>> SimilarityMatrix { get; set; } = new();

    public Dictionary<string, List<RecommendationResult>> Recommendations { get; set; } = new();

    public int TotalUsers { get; set; }

    public int TotalProducts { get; set; }
}