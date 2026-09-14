using Ecommerce.API.DTO.Auth;
using Ecommerce.API.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDTO model)
    {
        var fullName = model.FullName.Trim();
        var email = model.Email.Trim().ToLowerInvariant();

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            return BadRequest(new
            {
                message = "Email đã được sử dụng."
            });
        }

       
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            CreatedAt = DateTime.UtcNow
        };

        
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Đăng ký thất bại.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        
        var roleResult = await _userManager.AddToRoleAsync(user, "User");

        if (!roleResult.Succeeded)
        {
            return StatusCode(500, new
            {
                message = "Tạo tài khoản thành công nhưng không thể gán role."
            });
        }

        return Ok(new
        {
            message = "Đăng ký tài khoản thành công.",
            userId = user.Id,
            email = user.Email,
            fullName = user.FullName
        });
    }

    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO model)
    {
        
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Email hoặc mật khẩu không chính xác."
            });
        }

        
        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            model.Password,
            lockoutOnFailure: true
        );

        if (!result.Succeeded)
        {
            return Unauthorized(new
            {
                message = "Email hoặc mật khẩu không chính xác."
            });
        }

        
        var roles = await _userManager.GetRolesAsync(user);

        
        var token = GenerateJwtToken(user, roles);

        return Ok(new
        {
            message = "Đăng nhập thành công.",
            token = token,
            user = new
            {
                id = user.Id,
                email = user.Email,
                fullName = user.FullName,
                address = user.Address,
                sellerStatus = user.SellerStatus,
                roles = roles
            }
        });
    }

    
    private string GenerateJwtToken(
        ApplicationUser user,
        IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id
            ),

            new Claim(
                JwtRegisteredClaimNames.Email,
                user.Email ?? string.Empty
            ),

            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id
            ),

            new Claim(
                ClaimTypes.Name,
                user.UserName ?? string.Empty
            )
        };

        
        foreach (var role in roles)
        {
            claims.Add(
                new Claim(ClaimTypes.Role, role)
            );
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!
            )
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var duration = double.Parse(
            _configuration["Jwt:DurationInMinutes"] ?? "60"
        );

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(duration),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    [Authorize]
    [HttpPost("make-admin")]
    public async Task<IActionResult> MakeAdmin()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        if (!isAdmin)
        {
            var result = await _userManager.AddToRoleAsync(user, "Admin");

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
        }

        return Ok(new
        {
            message = "Đã cấp quyền Admin thành công.",
            email = user.Email,
            role = "Admin"
        });
    }
    
    [Authorize]
    [HttpPost("become-seller")]
    public async Task<IActionResult> BecomeSeller()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (await _userManager.IsInRoleAsync(user, "Admin") ||
            await _userManager.IsInRoleAsync(user, "Seller"))
        {
            return Ok(new { message = "Tài khoản đã có quyền bán hàng.", role = "Seller" });
        }

        var result = await _userManager.AddToRoleAsync(user, "Seller");
        if (!result.Succeeded)
            return BadRequest(new { message = "Không thể đăng ký trở thành người bán.", errors = result.Errors.Select(e => e.Description) });

        user.SellerStatus = "Approved";
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var newToken = GenerateJwtToken(user, roles);

        return Ok(new
        {
            message = "Đăng ký trở thành người bán thành công.",
            token = newToken,
            user = new { id = user.Id, email = user.Email, fullName = user.FullName, address = user.Address, sellerStatus = user.SellerStatus, roles }
        });
    }

}