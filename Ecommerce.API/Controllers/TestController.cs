using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    // =========================
    // TEST JWT
    // GET: /api/test/profile
    // =========================
    [Authorize]
    [HttpGet("profile")]
    public IActionResult Profile()
    {
        return Ok(new
        {
            message = "Bạn đã xác thực JWT thành công.",
            userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            email = User.FindFirst(ClaimTypes.Email)?.Value,
            roles = User.FindAll(ClaimTypes.Role)
                       .Select(x => x.Value)
        });
    }


    // =========================
    // CHỈ ADMIN
    // GET: /api/test/admin
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult Admin()
    {
        return Ok(new
        {
            message = "Bạn có quyền Admin.",
            email = User.FindFirst(ClaimTypes.Email)?.Value,
            roles = User.FindAll(ClaimTypes.Role)
                       .Select(x => x.Value)
        });
    }


    // =========================
    // CHỈ USER
    // GET: /api/test/user
    // =========================
    [Authorize(Roles = "User")]
    [HttpGet("user")]
    public IActionResult UserProfile()
    {
        return Ok(new
        {
            message = "Bạn có quyền User.",
            email = User.FindFirst(ClaimTypes.Email)?.Value,
            roles = User.FindAll(ClaimTypes.Role)
                       .Select(x => x.Value)
        });
    }
}   