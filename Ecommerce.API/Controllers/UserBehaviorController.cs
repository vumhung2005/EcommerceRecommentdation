using System.Security.Claims;
using Ecommerce.API.Data;
using Ecommerce.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/user-behaviors")]
[Authorize]
public class UserBehaviorController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserBehaviorController(AppDbContext context)
    {
        _context = context;
    }

    
    [HttpPost]
    public async Task<IActionResult> CreateBehavior(
        [FromBody] CreateUserBehaviorRequest request)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var validActions = new[]
        {
            "View",
            "Click",
            "AddToCart",
            "Purchase"
        };

        if (!validActions.Contains(
                request.ActionType,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Loại hành vi không hợp lệ.",
                validActions
            });
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(
                p => p.ProductId == request.ProductId);

        if (product == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy sản phẩm."
            });
        }

        var actionType = validActions.First(
            x => x.Equals(
                request.ActionType,
                StringComparison.OrdinalIgnoreCase));

        int weight = actionType switch
        {
            "View" => 1,
            "Click" => 2,
            "AddToCart" => 3,
            "Purchase" => 5,
            _ => 0
        };

        var behavior = new UserBehavior
        {
            UserId = userId,
            ProductId = request.ProductId,
            ActionType = actionType,
            Weight = weight,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserBehaviors.Add(behavior);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Ghi nhận hành vi thành công.",
            userBehaviorId = behavior.UserBehaviorId,
            userId = behavior.UserId,
            productId = behavior.ProductId,
            actionType = behavior.ActionType,
            weight = behavior.Weight,
            createdAt = behavior.CreatedAt
        });
    }


    
    [HttpGet("my")]
    public async Task<IActionResult> GetMyBehaviors()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var behaviors = await _context.UserBehaviors
            .Where(x => x.UserId == userId)
            .Include(x => x.Product)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                userBehaviorId = x.UserBehaviorId,
                productId = x.ProductId,

                productName = x.Product != null
                    ? x.Product.Name
                    : null,

                actionType = x.ActionType,
                weight = x.Weight,
                createdAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(behaviors);
    }


    
    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> GetProductBehaviors(
        int productId)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var productExists = await _context.Products
            .AnyAsync(p => p.ProductId == productId);

        if (!productExists)
        {
            return NotFound(new
            {
                message = "Không tìm thấy sản phẩm."
            });
        }

        var behaviors = await _context.UserBehaviors
            .Where(x =>
                x.UserId == userId &&
                x.ProductId == productId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                userBehaviorId = x.UserBehaviorId,
                productId = x.ProductId,
                actionType = x.ActionType,
                weight = x.Weight,
                createdAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(behaviors);
    }


    
    [HttpGet("summary")]
    public async Task<IActionResult> GetBehaviorSummary()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var summary = await _context.UserBehaviors
            .Where(x => x.UserId == userId)
            .GroupBy(x => x.ActionType)
            .Select(g => new
            {
                actionType = g.Key,
                count = g.Count(),
                totalWeight = g.Sum(x => x.Weight)
            })
            .OrderByDescending(x => x.totalWeight)
            .ToListAsync();

        return Ok(summary);
    }


    
    [HttpGet("product-score")]
    public async Task<IActionResult> GetProductScores()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _context.UserBehaviors
            .Where(x => x.UserId == userId)
            .GroupBy(x => new
            {
                x.ProductId,
                ProductName = x.Product!.Name
            })
            .Select(g => new
            {
                productId = g.Key.ProductId,
                productName = g.Key.ProductName,

                totalScore = g.Sum(x => x.Weight),

                totalActions = g.Count()
            })
            .OrderByDescending(x => x.totalScore)
            .ToListAsync();

        return Ok(result);
    }
}



public class CreateUserBehaviorRequest
{
    public int ProductId { get; set; }

    public string ActionType { get; set; } = string.Empty;
}