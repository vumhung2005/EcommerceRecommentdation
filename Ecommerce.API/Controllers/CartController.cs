using System.Security.Claims;
using Ecommerce.API.Data;
using Ecommerce.API.Models;
using Ecommerce.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserBehaviorService _behaviorService;

    public CartController(
        AppDbContext context,
        UserBehaviorService behaviorService)
    {
        _context = context;
        _behaviorService = behaviorService;
    }

    
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return Ok(new
            {
                cartId = 0,
                userId,
                items = new List<object>(),
                totalAmount = 0
            });
        }

        var items = cart.CartItems.Select(item => new
        {
            cartItemId = item.CartItemId,
            productId = item.ProductId,
            productName = item.Product!.Name,
            price = item.Product.Price,
            quantity = item.Quantity,
            image = item.Product.Image,
            subtotal = item.Product.Price * item.Quantity
        }).ToList();

        var totalAmount = items.Sum(x => x.subtotal);

        return Ok(new
        {
            cartId = cart.CartId,
            userId = cart.UserId,
            items,
            totalAmount
        });
    }


    
    [HttpPost("items")]
    public async Task<IActionResult> AddToCart(
        [FromBody] AddCartItemRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (request.Quantity <= 0)
        {
            return BadRequest(new
            {
                message = "Số lượng phải lớn hơn 0."
            });
        }

        
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == request.ProductId);

        if (product == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy sản phẩm."
            });
        }

        
        if (product.Stock < request.Quantity)
        {
            return BadRequest(new
            {
                message =
                    $"Sản phẩm '{product.Name}' không đủ tồn kho. " +
                    $"Tồn kho hiện tại: {product.Stock}."
            });
        }

        
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId
            };

            _context.Carts.Add(cart);

            await _context.SaveChangesAsync();
        }

        
        var cartItem = cart.CartItems
            .FirstOrDefault(ci => ci.ProductId == request.ProductId);

        if (cartItem != null)
        {
            var newQuantity = cartItem.Quantity + request.Quantity;

            if (newQuantity > product.Stock)
            {
                return BadRequest(new
                {
                    message =
                        $"Số lượng vượt quá tồn kho. " +
                        $"Tồn kho hiện tại: {product.Stock}."
                });
            }

            cartItem.Quantity = newQuantity;
        }
        else
        {
            cartItem = new CartItem
            {
                CartId = cart.CartId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };

            _context.CartItems.Add(cartItem);
        }

       
        await _context.SaveChangesAsync();
        
        await _behaviorService.AddBehaviorAsync(
            userId,
            request.ProductId,
            "AddToCart");

        

        
        return Ok(new
        {
            message = "Thêm sản phẩm vào giỏ hàng thành công.",

            cartItemId = cartItem.CartItemId,

            cartId = cart.CartId,

            productId = product.ProductId,

            productName = product.Name,

            price = product.Price,

            quantity = cartItem.Quantity,

            subtotal = product.Price * cartItem.Quantity,

            behavior = new
            {
                actionType = "AddToCart",
                weight = 3
            }
        });
    }


    
    [HttpPut("items/{cartItemId:int}")]
    public async Task<IActionResult> UpdateCartItem(
        int cartItemId,
        [FromBody] UpdateCartItemRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (request.Quantity <= 0)
        {
            return BadRequest(new
            {
                message = "Số lượng phải lớn hơn 0."
            });
        }

        var cartItem = await _context.CartItems
            .Include(ci => ci.Cart)
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci =>
                ci.CartItemId == cartItemId &&
                ci.Cart!.UserId == userId);

        if (cartItem == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy sản phẩm trong giỏ hàng."
            });
        }

        if (cartItem.Product == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy sản phẩm."
            });
        }

        if (request.Quantity > cartItem.Product.Stock)
        {
            return BadRequest(new
            {
                message =
                    $"Số lượng vượt quá tồn kho. " +
                    $"Tồn kho hiện tại: {cartItem.Product.Stock}."
            });
        }

        cartItem.Quantity = request.Quantity;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Cập nhật giỏ hàng thành công.",
            cartItemId = cartItem.CartItemId,
            quantity = cartItem.Quantity
        });
    }


    
    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveCartItem(int cartItemId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var cartItem = await _context.CartItems
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci =>
                ci.CartItemId == cartItemId &&
                ci.Cart!.UserId == userId);

        if (cartItem == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy sản phẩm trong giỏ hàng."
            });
        }

        _context.CartItems.Remove(cartItem);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Xóa sản phẩm khỏi giỏ hàng thành công."
        });
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return NotFound(new
            {
                message = "Giỏ hàng không tồn tại."
            });
        }

        _context.CartItems.RemoveRange(cart.CartItems);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Đã xóa toàn bộ giỏ hàng."
        });
    }
}



public class AddCartItemRequest
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }
}



public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}