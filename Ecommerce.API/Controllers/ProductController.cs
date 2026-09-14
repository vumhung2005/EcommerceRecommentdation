using System.Security.Claims;
using Ecommerce.API.Data;
using Ecommerce.API.Models;
using Ecommerce.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserBehaviorService _behaviorService;

    public ProductController(
        AppDbContext context,
        UserBehaviorService behaviorService)
    {
        _context = context;
        _behaviorService = behaviorService;
    }

    // =====================================================
    // GET: api/products
    // Lấy danh sách sản phẩm
    // =====================================================
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .ToListAsync();

        return Ok(products);
    }


    // =====================================================
    // GET: api/products/{id}
    // Lấy chi tiết sản phẩm
    // Tự động ghi hành vi View nếu User đăng nhập
    // =====================================================
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy sản phẩm."
            });
        }

        // =================================================
        // Ghi hành vi VIEW
        // Chỉ ghi khi User đã đăng nhập
        // Không ghi hành vi của Admin
        // =================================================
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(userId) &&
            !User.IsInRole("Admin"))
        {
            await _behaviorService.AddBehaviorAsync(
                userId,
                product.ProductId,
                "View"
            );
        }

        return Ok(new
        {
            productId = product.ProductId,
            categoryId = product.CategoryId,
            brandId = product.BrandId,
            name = product.Name,
            description = product.Description,
            price = product.Price,
            stock = product.Stock,
            image = product.Image,
            category = product.Category,
            brand = product.Brand
        });
    }

    // =====================================================
    // POST: api/products/{id}/click
    // Ghi nhận User click vào sản phẩm
    // =====================================================
    [Authorize]
    [HttpPost("{id:int}/click")]
    public async Task<IActionResult> ClickProduct(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy sản phẩm."
            });
        }

        await _behaviorService.AddBehaviorAsync(
            userId,
            product.ProductId,
            "Click"
        );

        return Ok(new
        {
            message = "Ghi nhận hành vi Click thành công.",
            productId = product.ProductId,
            actionType = "Click",
            weight = 2
        });
    }
    // =====================================================
    // POST: api/products
    // Admin thêm sản phẩm
    // =====================================================
    [Authorize(Roles = "Admin,Seller")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        [FromBody] Product product)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.CategoryId == product.CategoryId);

        if (!categoryExists)
        {
            return BadRequest(new
            {
                message = "Category không tồn tại."
            });
        }

        var brandExists = await _context.Brands
            .AnyAsync(b => b.BrandId == product.BrandId);

        if (!brandExists)
        {
            return BadRequest(new
            {
                message = "Brand không tồn tại."
            });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        product.OwnerId = userId;
        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.ProductId },
            new
            {
                message = "Thêm sản phẩm thành công.",
                productId = product.ProductId,
                categoryId = product.CategoryId,
                brandId = product.BrandId,
                name = product.Name,
                description = product.Description,
                price = product.Price,
                stock = product.Stock,
                image = product.Image
            }
        );
    }


    // =====================================================
    // PUT: api/products/{id}
    // Admin cập nhật sản phẩm
    // =====================================================
    [Authorize(Roles = "Admin,Seller")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(
        int id,
        [FromBody] Product product)
    {
        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (existingProduct == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy sản phẩm."
            });
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && existingProduct.OwnerId != currentUserId)
        {
            return Forbid();
        }

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.CategoryId == product.CategoryId);

        if (!categoryExists)
        {
            return BadRequest(new
            {
                message = "Category không tồn tại."
            });
        }

        var brandExists = await _context.Brands
            .AnyAsync(b => b.BrandId == product.BrandId);

        if (!brandExists)
        {
            return BadRequest(new
            {
                message = "Brand không tồn tại."
            });
        }

        existingProduct.CategoryId = product.CategoryId;
        existingProduct.BrandId = product.BrandId;
        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.Stock = product.Stock;
        existingProduct.Image = product.Image;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Cập nhật sản phẩm thành công.",
            productId = existingProduct.ProductId,
            categoryId = existingProduct.CategoryId,
            brandId = existingProduct.BrandId,
            name = existingProduct.Name,
            description = existingProduct.Description,
            price = existingProduct.Price,
            stock = existingProduct.Stock,
            image = existingProduct.Image
        });
    }

    // =====================================================
// POST: api/products/upload-image
// Admin upload hình ảnh sản phẩm
// =====================================================
    [Authorize(Roles = "Admin,Seller")]
    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "Vui lòng chọn hình ảnh."
            });
        }

        // Kiểm tra định dạng
        var allowedExtensions = new[]
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        var extension =
            Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new
            {
                message = "Chỉ chấp nhận JPG, JPEG, PNG hoặc WEBP."
            });
        }

        // Giới hạn 5MB
        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new
            {
                message = "Kích thước hình ảnh không được vượt quá 5MB."
            });
        }

        // Tạo tên file mới để tránh trùng
        var fileName =
            $"{Guid.NewGuid()}{extension}";

        // Đường dẫn wwwroot/images/products
        var folderPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "images",
            "products"
        );

        // Nếu thư mục chưa tồn tại thì tạo
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var filePath = Path.Combine(
            folderPath,
            fileName
        );

        // Lưu file
        using (var stream = new FileStream(
            filePath,
            FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // URL trả về cho React
        var imageUrl =
            $"{Request.Scheme}://{Request.Host}/images/products/{fileName}";

        return Ok(new
        {
            message = "Upload hình ảnh thành công.",
            fileName = fileName,
            imageUrl = imageUrl
        });
    }
    // =====================================================
    // DELETE: api/products/{id}
    // Admin xóa sản phẩm
    // =====================================================
    [Authorize(Roles = "Admin,Seller")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy sản phẩm."
            });
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && product.OwnerId != currentUserId)
        {
            return Forbid();
        }

        _context.Products.Remove(product);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(new
            {
                message =
                    "Không thể xóa sản phẩm vì sản phẩm đang được sử dụng " +
                    "trong đơn hàng, giỏ hàng hoặc dữ liệu liên quan."
            });
        }

        return Ok(new
        {
            message = "Xóa sản phẩm thành công."
        });
    }
}