using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controlllers;
[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Welcome to the Ecommerce API!");
    }
}