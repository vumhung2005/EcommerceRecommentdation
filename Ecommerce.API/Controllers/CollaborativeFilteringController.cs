using Ecommerce.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/collaborative-filtering")]
public class CollaborativeFilteringController : ControllerBase
{
    private readonly CollaborativeFilteringService _service;

    public CollaborativeFilteringController(
        CollaborativeFilteringService service)
    {
        _service = service;
    }

    
    [HttpGet("matrix")]
    public async Task<IActionResult> GetMatrix()
    {
        var matrix = await _service.BuildDemoMatrixAsync();

        return Ok(matrix);
    }

    
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserVector(
        string userId)
    {
        var vector = await _service.GetUserVectorAsync(userId);

        return Ok(vector);
    }

    
    [HttpGet("similar-users/{userId}")]
    public async Task<IActionResult> GetSimilarUsers(
        string userId,
        int topN = 5)
    {
        var result = await _service.GetSimilarUsersAsync(
            userId,
            topN
        );

        return Ok(result);
    }
}