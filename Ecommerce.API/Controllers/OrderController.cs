using System.Security.Claims;
using Ecommerce.API.Data;
using Ecommerce.API.DTO.Order;
using Ecommerce.API.Models;
using Ecommerce.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserBehaviorService _behaviorService;

    public OrderController(
        AppDbContext context,
        UserBehaviorService behaviorService)
    {
        _context = context;
        _behaviorService = behaviorService;
    }

    
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        IQueryable<Order> query = _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Include(o => o.Payment);

        
        if (!User.IsInRole("Admin"))
        {
            query = query.Where(o => o.UserId == userId);
        }

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new
            {
                o.OrderId,
                o.UserId,
                o.TotalAmount,
                o.Status,
                o.OrderDate,
                o.ReceiverName,
                o.Phone,
                o.ShippingAddress,
                o.PaymentMethod,
                Payment = o.Payment == null
                    ? null
                    : new
                    {
                        o.Payment.PaymentId,
                        o.Payment.Status,
                        o.Payment.PaymentMethod,
                        o.Payment.Amount,
                        o.Payment.PaymentDate
                    },
                OrderDetails = o.OrderDetails.Select(d => new
                {
                    d.OrderDetailId,
                    d.ProductId,
                    d.Quantity,
                    d.Price,
                    Product = d.Product == null
                        ? null
                        : new
                        {
                            d.Product.ProductId,
                            d.Product.Name,
                            d.Product.Price,
                            d.Product.Image
                        }
                })
            })
            .ToListAsync();

        return Ok(orders);
    }


    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var order = await _context.Orders
            .Where(o => o.OrderId == id)
            .Select(o => new
            {
                o.OrderId,
                o.UserId,
                o.TotalAmount,
                o.Status,
                o.OrderDate,
                o.ReceiverName,
                o.Phone,
                o.ShippingAddress,
                o.PaymentMethod,
                Payment = o.Payment == null
                    ? null
                    : new
                    {
                        o.Payment.PaymentId,
                        o.Payment.Status,
                        o.Payment.PaymentMethod,
                        o.Payment.Amount,
                        o.Payment.PaymentDate
                    },
                OrderDetails = o.OrderDetails.Select(d => new
                {
                    d.OrderDetailId,
                    d.ProductId,
                    d.Quantity,
                    d.Price,
                    Product = d.Product == null
                        ? null
                        : new
                        {
                            d.Product.ProductId,
                            d.Product.Name,
                            d.Product.Price,
                            d.Product.Image
                        }
                })
            })
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy đơn hàng."
            });
        }

       
        if (!User.IsInRole("Admin") && order.UserId != userId)
        {
            return Forbid();
        }

        return Ok(order);
    }


   
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (request == null || string.IsNullOrWhiteSpace(request.ReceiverName) ||
            string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.ShippingAddress))
        {
            return BadRequest(new { message = "Vui lòng nhập đầy đủ người nhận, số điện thoại và địa chỉ giao hàng." });
        }

        var validPaymentMethods = new[] { "COD", "BankTransfer" };
        if (!validPaymentMethods.Contains(request.PaymentMethod, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Phương thức thanh toán không hợp lệ.", validPaymentMethods });

        
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return BadRequest(new
            {
                message = "Giỏ hàng không tồn tại."
            });
        }

        if (cart.CartItems.Count == 0)
        {
            return BadRequest(new
            {
                message = "Giỏ hàng đang trống."
            });
        }

       
        var itemsToCheckout = cart.CartItems.AsEnumerable();

        if (request.CartItemIds != null && request.CartItemIds.Count > 0)
        {
            var idSet = request.CartItemIds.ToHashSet();
            itemsToCheckout = cart.CartItems.Where(ci => idSet.Contains(ci.CartItemId));

            if (!itemsToCheckout.Any())
            {
                return BadRequest(new
                {
                    message = "Không tìm thấy sản phẩm đã chọn trong giỏ hàng."
                });
            }
        }

        var checkoutItems = itemsToCheckout.ToList();

        
        foreach (var item in checkoutItems)
        {
            if (item.Product == null)
            {
                return BadRequest(new
                {
                    message =
                        $"Không tìm thấy sản phẩm ID {item.ProductId}."
                });
            }

            if (item.Quantity <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Số lượng sản phẩm phải lớn hơn 0."
                });
            }

            if (item.Product.Stock < item.Quantity)
            {
                return BadRequest(new
                {
                    message =
                        $"Sản phẩm '{item.Product.Name}' " +
                        $"không đủ tồn kho. " +
                        $"Tồn kho hiện tại: {item.Product.Stock}."
                });
            }
        }

        
        decimal totalAmount = checkoutItems.Sum(
            item => item.Product!.Price * item.Quantity
        );

        
        var order = new Order
        {
            UserId = userId,
            TotalAmount = totalAmount,
            Status = "Pending",
            OrderDate = DateTime.UtcNow,
            ReceiverName = request.ReceiverName.Trim(),
            Phone = request.Phone.Trim(),
            ShippingAddress = request.ShippingAddress.Trim(),
            PaymentMethod = request.PaymentMethod.Trim()
        };

        _context.Orders.Add(order);

        
        await _context.SaveChangesAsync();


        _context.Payments.Add(new Payment
        {
            OrderId = order.OrderId,
            PaymentMethod = request.PaymentMethod.Trim(),
            Status = "Pending",
            Amount = order.TotalAmount,
            PaymentDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        
        foreach (var item in checkoutItems)
        {
            var product = item.Product!;

            var orderDetail = new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = product.ProductId,
                Quantity = item.Quantity,
                Price = product.Price
            };

            _context.OrderDetails.Add(orderDetail);

            await _behaviorService.AddBehaviorAsync(
                userId,
                product.ProductId,
                "Purchase");
        }

        _context.CartItems.RemoveRange(checkoutItems);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetOrder),
            new { id = order.OrderId },
            new
            {
                message = "Tạo đơn hàng thành công.",
                orderId = order.OrderId,
                totalAmount = order.TotalAmount,
                status = order.Status,
                orderDate = order.OrderDate
            }
        );
    }


    
    [Authorize(Roles = "Admin,Seller")]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        int id,
        [FromBody] UpdateOrderStatusRequest request)
    {
        if (request == null)
        {
            return BadRequest(new
            {
                message = "Dữ liệu cập nhật trạng thái không hợp lệ."
            });
        }

        var requestedStatus = request.Status;
        if (string.IsNullOrWhiteSpace(requestedStatus) && Request.HasFormContentType)
        {
            requestedStatus = Request.Form["Status"].ToString();
        }

        if (string.IsNullOrWhiteSpace(requestedStatus))
        {
            return BadRequest(new
            {
                message = "Status không được để trống."
            });
        }

        request.Status = requestedStatus.Trim();


        
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null)
        {
            return NotFound(new
            {
                message =
                    $"Không tìm thấy đơn hàng có ID = {id}."
            });
        }


        
        var validStatuses = new[]
        {
            "Pending",
            "Confirmed",
            "Shipping",
            "Completed",
            "Cancelled"
        };


        
        var newStatus = validStatuses.FirstOrDefault(
            s => s.Equals(
                request.Status.Trim(),
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (newStatus == null)
        {
            return BadRequest(new
            {
                message =
                    "Trạng thái đơn hàng không hợp lệ.",

                validStatuses
            });
        }

        var allowedTransition = (order.Status, newStatus) switch
        {
            ("Pending", "Confirmed") => true,
            ("Pending", "Completed") => true,
            ("Pending", "Cancelled") => true,
            ("Confirmed", "Shipping") => true,
            ("Confirmed", "Completed") => true,
            ("Confirmed", "Cancelled") => true,
            ("Shipping", "Completed") => true,
            ("Shipping", "Cancelled") => true,
            _ => false
        };

        if (!allowedTransition)
            return BadRequest(new { message = $"Không thể chuyển đơn từ {order.Status} sang {newStatus}." });

        
        var isFirstConfirmation = newStatus == "Confirmed" || (newStatus == "Completed" && order.Status == "Pending");
        if (isFirstConfirmation)
        {
            var details = await _context.OrderDetails
                .Include(d => d.Product)
                .Where(d => d.OrderId == order.OrderId)
                .ToListAsync();

            foreach (var detail in details)
            {
                if (detail.Product == null || detail.Product.Stock < detail.Quantity)
                    return BadRequest(new { message = $"Sản phẩm '{detail.Product?.Name ?? detail.ProductId.ToString()}' không đủ tồn kho." });
            }

            foreach (var detail in details) detail.Product!.Stock -= detail.Quantity;
        }

        order.Status = newStatus;

        if (newStatus == "Completed")
        {
            var completedProducts = await _context.OrderDetails
                .Where(d => d.OrderId == order.OrderId)
                .Select(d => d.ProductId)
                .ToListAsync();
            foreach (var productId in completedProducts)
                await _behaviorService.AddBehaviorAsync(order.UserId, productId, "Purchase");
        }

        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == order.OrderId);
        if (payment != null)
        {
            if (newStatus == "Completed") payment.Status = "Paid";
            if (newStatus == "Cancelled") payment.Status = "Cancelled";
        }

        await _context.SaveChangesAsync();


        
        return Ok(new
        {
            message =
                "Cập nhật trạng thái đơn hàng thành công.",

            orderId = order.OrderId,

            status = order.Status
        });
    }
}



public class CreateOrderRequest
{
    public string ReceiverName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "COD";

    public List<int>? CartItemIds { get; set; }
}
