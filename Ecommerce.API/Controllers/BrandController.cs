using Ecommerce.API.Data;
using Ecommerce.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/brands")]
public class BrandController : ControllerBase
{
    private readonly AppDbContext _context;

    public BrandController(AppDbContext context)
    {
        _context = context;
    }

    
    [HttpGet]
    public async Task<IActionResult> GetBrands()
    {
        var brands = await _context.Brands
            .OrderBy(b => b.BrandName)
            .ToListAsync();

        return Ok(brands);
    }

    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBrand(int id)
    {
        var brand = await _context.Brands
            .Include(b => b.Products)
            .FirstOrDefaultAsync(b => b.BrandId == id);

        if (brand == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy thương hiệu."
            });
        }

        return Ok(brand);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateBrand(Brand brand)
    {
        var exists = await _context.Brands
            .AnyAsync(b => b.BrandName == brand.BrandName);

        if (exists)
        {
            return BadRequest(new
            {
                message = "Thương hiệu đã tồn tại."
            });
        }

        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetBrand),
            new { id = brand.BrandId },
            brand
        );
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBrand(
        int id,
        Brand brand)
    {
        var existingBrand = await _context.Brands
            .FirstOrDefaultAsync(b => b.BrandId == id);

        if (existingBrand == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy thương hiệu."
            });
        }

        existingBrand.BrandName = brand.BrandName;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Cập nhật thương hiệu thành công.",
            brand = existingBrand
        });
    }

    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.BrandId == id);

        if (brand == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy thương hiệu."
            });
        }

        var hasProducts = await _context.Products
            .AnyAsync(p => p.BrandId == id);

        if (hasProducts)
        {
            return BadRequest(new
            {
                message = "Không thể xóa thương hiệu vì đang có sản phẩm thuộc thương hiệu này."
            });
        }

        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Xóa thương hiệu thành công."
        });
    }
}