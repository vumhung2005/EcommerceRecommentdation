using Ecommerce.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly AppDbContext _context;
    public AdminDashboardController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var completed = _context.Orders.Where(o => o.Status == "Completed");
        var revenue = await completed.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
        var orders = await _context.Orders.CountAsync();
        var pending = await _context.Orders.CountAsync(o => o.Status == "Pending");
        var users = await _context.Users.CountAsync();
        var products = await _context.Products.CountAsync();
        var stock = await _context.Products.SumAsync(p => (int?)p.Stock) ?? 0;

        var start = DateTime.UtcNow.Date.AddDays(-29);
        var rows = await completed.Where(o => o.OrderDate >= start)
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new { date = g.Key, revenue = g.Sum(o => o.TotalAmount), orders = g.Count() })
            .OrderBy(x => x.date).ToListAsync();

        var chart = Enumerable.Range(0, 30).Select(i => start.AddDays(i)).Select(d =>
        {
            var x = rows.FirstOrDefault(r => r.date == d);
            return new { date = d.ToString("yyyy-MM-dd"), revenue = x?.revenue ?? 0m, orders = x?.orders ?? 0 };
        });

        return Ok(new { revenue, orders, pending, users, products, stock, chart });
    }
}
