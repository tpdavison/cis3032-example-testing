using System;
using System.Net;

namespace Movies.Web.Services.Reviews;

public class ReviewsService : IReviewsService
{
    private readonly HttpClient _client;

    public ReviewsService(HttpClient client,
                          IConfiguration configuration)
    {
        var baseUrl = configuration["WebServices:Reviews:BaseURL"];
        client.BaseAddress = new System.Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(5);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        _client = client;
    }

    public async Task<ReviewDto> GetReviewAsync(int id)
    {
        var response = await _client.GetAsync("api/reviews/" + id);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        var review = await response.Content.ReadAsAsync<ReviewDto>();
        return review;
    }

    public async Task<IEnumerable<ReviewDto>> GetReviewsAsync(string subject)
    {
        var uri = "api/reviews?category=MOV";
        if (subject != null)
        {
            uri = uri + "&subject=" + subject;
        }
        var response = await _client.GetAsync(uri);
        response.EnsureSuccessStatusCode();
        var reviews = await response.Content.ReadAsAsync<IEnumerable<ReviewDto>>();
        return reviews;
    }
}
