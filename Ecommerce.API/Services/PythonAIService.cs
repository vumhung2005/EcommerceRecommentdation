using Ecommerce.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace Ecommerce.API.Services;

public class PythonAIService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _http;

    public PythonAIService(AppDbContext context)
    {
        _context = context;

        _http = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:8000/")
        };
    }

    public async Task<object> GetRecommendations(string userId)
    {
       
        var behaviors = await _context.UserBehaviors
            .AsNoTracking()
            .ToListAsync();


        

        var request = new
        {
            user_id = userId,

            interactions = behaviors.Select(x => new
            {
                user_id = x.UserId,
                product_id = x.ProductId,

                // Weight chính là score
                score = x.Weight
            }).ToList()
        };


        

        var json = JsonSerializer.Serialize(request);


       

        var response = await _http.PostAsync(
            "recommend",
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            )
        );


       
        var result = await response.Content.ReadAsStringAsync();


        

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Python AI Service lỗi: " +
                $"{(int)response.StatusCode} - {result}"
            );
        }


        

        return JsonSerializer.Deserialize<object>(result)!;
    }
}