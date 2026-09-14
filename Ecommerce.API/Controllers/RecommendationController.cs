using System.Security.Claims;

using Ecommerce.API.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/recommendations")]
[Authorize]
public class RecommendationController : ControllerBase
{
    private readonly CollaborativeFilteringService _collaborativeService;
    private readonly PythonAIService _pythonAIService;

    public RecommendationController(
        CollaborativeFilteringService collaborativeService,
        PythonAIService pythonAIService)
    {
        _collaborativeService = collaborativeService;
        _pythonAIService = pythonAIService;
    }


    

    [HttpGet("collaborative")]
    public async Task<IActionResult> GetCollaborativeRecommendations(
        [FromQuery] int topUsers = 5,
        [FromQuery] int topProducts = 10)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var recommendations =
            await _collaborativeService
                .GenerateRecommendationsAsync(
                    userId,
                    topUsers,
                    topProducts);

        return Ok(new
        {
            userId,

            algorithm =
                "User-Based Collaborative Filtering",

            similarity =
                "Cosine Similarity",

            topUsers,
            topProducts,

            count =
                recommendations.Count,

            recommendations
        });
    }


    

    [HttpPost("collaborative/generate")]
    public async Task<IActionResult>
        GenerateCollaborativeRecommendations(
            [FromQuery] int topUsers = 5,
            [FromQuery] int topProducts = 10)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var count =
            await _collaborativeService
                .SaveRecommendationsAsync(
                    userId,
                    topUsers,
                    topProducts);

        return Ok(new
        {
            message =
                "Đã tạo và lưu recommendation bằng Collaborative Filtering.",

            userId,

            algorithm =
                "User-Based Collaborative Filtering",

            similarity =
                "Cosine Similarity",

            count
        });
    }


   

    [HttpGet("collaborative/saved")]
    public async Task<IActionResult>
        GetSavedCollaborativeRecommendations(
            [FromQuery] int topProducts = 10)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var recommendations =
            await _collaborativeService
                .GetSavedRecommendationsAsync(
                    userId,
                    topProducts);

        return Ok(new
        {
            userId,

            algorithm =
                "User-Based Collaborative Filtering",

            count =
                recommendations.Count,

            recommendations
        });
    }


    

    [HttpGet("collaborative/similar-users")]
    public async Task<IActionResult> GetSimilarUsers(
        [FromQuery] int topUsers = 5)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var users =
            await _collaborativeService
                .GetSimilarUsersAsync(
                    userId,
                    topUsers);

        return Ok(new
        {
            userId,

            algorithm =
                "Cosine Similarity",

            count =
                users.Count,

            similarUsers =
                users
        });
    }



    [HttpGet("python")]
    public async Task<IActionResult>
        GetPythonRecommendations()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _pythonAIService
                    .GetRecommendations(userId);

            return Ok(new
            {
                userId,

                source =
                    "Python FastAPI",

                algorithm =
                    "User-Based Collaborative Filtering",

                similarity =
                    "Cosine Similarity",

                result
            });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        "Không thể kết nối đến Python AI Service.",

                    pythonUrl =
                        "http://127.0.0.1:8000",

                    error =
                        ex.Message
                });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Lỗi khi gọi Python AI Service.",

                    error =
                        ex.Message
                });
        }
    }
}