using Ecommerce.API.Data;
using Ecommerce.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/categories
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        return Ok(categories);
    }

    // GET: api/categories/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy danh mục."
            });
        }

        return Ok(category);
    }

    // POST: api/categories
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCategory(Category category)
    {
        var exists = await _context.Categories
            .AnyAsync(c => c.CategoryName == category.CategoryName);

        if (exists)
        {
            return BadRequest(new
            {
                message = "Danh mục đã tồn tại."
            });
        }

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.CategoryId },
            category
        );
    }

    // PUT: api/categories/{id}
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(
        int id,
        Category category)
    {
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (existingCategory == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy danh mục."
            });
        }

        existingCategory.CategoryName = category.CategoryName;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Cập nhật danh mục thành công.",
            category = existingCategory
        });
    }

    // DELETE: api/categories/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy danh mục."
            });
        }

        var hasProducts = await _context.Products
            .AnyAsync(p => p.CategoryId == id);

        if (hasProducts)
        {
            return BadRequest(new
            {
                message = "Không thể xóa danh mục vì đang có sản phẩm thuộc danh mục này."
            });
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Xóa danh mục thành công."
        });
    }
}