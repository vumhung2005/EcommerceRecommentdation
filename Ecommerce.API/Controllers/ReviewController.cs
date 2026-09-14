using System.Security.Claims;
using Ecommerce.API.Data;
using Ecommerce.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReviewController(AppDbContext context)
    {
        _context = context;
    }

    // =====================================================
    // GET: api/reviews/product/{productId}
    // Xem tất cả đánh giá của một sản phẩm
    // =====================================================
    [AllowAnonymous]
    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> GetProductReviews(int productId)
    {
        var productExists = await _context.Products
            .AnyAsync(p => p.ProductId == productId);

        if (!productExists)
        {
            return NotFound(new
            {
                message = "Không tìm thấy sản phẩm."
            });
        }

        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.ReviewId)
            .Select(r => new
            {
                reviewId = r.ReviewId,
                userId = r.UserId,
                productId = r.ProductId,
                rating = r.Rating,
                comment = r.Comment
            })
            .ToListAsync();

        var averageRating = reviews.Count > 0
            ? reviews.Average(r => r.rating)
            : 0;

        return Ok(new
        {
            productId,
            totalReviews = reviews.Count,
            averageRating,
            reviews
        });
    }



    [HttpGet("can-review/{productId:int}")]
    public async Task<IActionResult> CanReview(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Ok(new { canReview = false });

        var canReview = await _context.Orders
            .Include(o => o.OrderDetails)
            .Include(o => o.Payment)
            .AnyAsync(o => o.UserId == userId && o.Status == "Completed" &&
                           o.Payment != null && o.Payment.Status == "Paid" &&
                           o.OrderDetails.Any(d => d.ProductId == productId));
        return Ok(new { canReview });
    }

    // =====================================================
    // GET: api/reviews/my
    // Xem các đánh giá của User hiện tại
    // =====================================================
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReviews()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var reviews = await _context.Reviews
            .Where(r => r.UserId == userId)
            .Include(r => r.Product)
            .OrderByDescending(r => r.ReviewId)
            .Select(r => new
            {
                reviewId = r.ReviewId,
                productId = r.ProductId,
                productName = r.Product != null
                    ? r.Product.Name
                    : null,
                rating = r.Rating,
                comment = r.Comment
            })
            .ToListAsync();

        return Ok(reviews);
    }


    // =====================================================
    // POST: api/reviews
    // Thêm đánh giá sản phẩm
    // =====================================================
    [HttpPost]
    public async Task<IActionResult> CreateReview(
        [FromBody] CreateReviewRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Kiểm tra Rating
        if (request.Rating < 1 || request.Rating > 5)
        {
            return BadRequest(new
            {
                message = "Đánh giá phải từ 1 đến 5 sao."
            });
        }

        // Kiểm tra Product
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == request.ProductId);

        if (product == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy sản phẩm."
            });
        }

        // Chỉ được đánh giá sau khi đơn hàng của User đã hoàn thành và thanh toán
        var canReview = await _context.Orders
            .Include(o => o.OrderDetails)
            .Include(o => o.Payment)
            .AnyAsync(o => o.UserId == userId &&
                           o.Status == "Completed" &&
                           (o.Payment != null && o.Payment.Status == "Paid") &&
                           o.OrderDetails.Any(d => d.ProductId == request.ProductId));

        if (!canReview)
        {
            return BadRequest(new { message = "Bạn chỉ có thể đánh giá sản phẩm sau khi đơn hàng đã hoàn thành và thanh toán." });
        }

        // Kiểm tra User đã đánh giá sản phẩm chưa
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.ProductId == request.ProductId);

        if (existingReview != null)
        {
            return BadRequest(new
            {
                message = "Bạn đã đánh giá sản phẩm này rồi."
            });
        }

        var review = new Review
        {
            UserId = userId,
            ProductId = request.ProductId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _context.Reviews.Add(review);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProductReviews),
            new
            {
                productId = review.ProductId
            },
            new
            {
                message = "Đánh giá sản phẩm thành công.",
                reviewId = review.ReviewId,
                productId = review.ProductId,
                rating = review.Rating,
                comment = review.Comment
            }
        );
    }


    // =====================================================
    // PUT: api/reviews/{id}
    // User sửa đánh giá của mình
    // =====================================================
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateReview(
        int id,
        [FromBody] UpdateReviewRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (request.Rating < 1 || request.Rating > 5)
        {
            return BadRequest(new
            {
                message = "Đánh giá phải từ 1 đến 5 sao."
            });
        }

        var review = await _context.Reviews
            .FirstOrDefaultAsync(r =>
                r.ReviewId == id &&
                r.UserId == userId);

        if (review == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy đánh giá của bạn."
            });
        }

        review.Rating = request.Rating;
        review.Comment = request.Comment;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Cập nhật đánh giá thành công.",
            reviewId = review.ReviewId,
            productId = review.ProductId,
            rating = review.Rating,
            comment = review.Comment
        });
    }


    // =====================================================
    // DELETE: api/reviews/{id}
    // User xóa đánh giá của mình
    // =====================================================
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var review = await _context.Reviews
            .FirstOrDefaultAsync(r =>
                r.ReviewId == id &&
                r.UserId == userId);

        if (review == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy đánh giá của bạn."
            });
        }

        _context.Reviews.Remove(review);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Xóa đánh giá thành công."
        });
    }
}


// =========================================================
// DTO: Tạo Review
// =========================================================
public class CreateReviewRequest
{
    public int ProductId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }
}


// =========================================================
// DTO: Cập nhật Review
// =========================================================
public class UpdateReviewRequest
{
    public int Rating { get; set; }

    public string? Comment { get; set; }
}