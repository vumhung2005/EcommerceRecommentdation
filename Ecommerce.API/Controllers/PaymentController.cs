using System.Security.Claims;
using Ecommerce.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _context;
    public PaymentController(AppDbContext context) => _context = context;

    [HttpPost("{orderId:int}/pay")]
    public async Task<IActionResult> Pay(int orderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var order = await _context.Orders.Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
        if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });
        if (!User.IsInRole("Admin") && order.UserId != userId) return Forbid();
        if (order.Status == "Cancelled") return BadRequest(new { message = "Đơn hàng đã bị hủy." });
        if (order.Payment == null) return BadRequest(new { message = "Đơn hàng chưa có thông tin thanh toán." });

        order.Payment.Status = "Paid";
        order.Payment.PaymentDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Thanh toán thành công.", orderId, status = order.Payment.Status });
    }
}
