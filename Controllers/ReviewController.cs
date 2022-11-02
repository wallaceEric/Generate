using Microsoft.AspNetCore.Mvc;
using generate.Services;

namespace generate.Controllers;

[ApiController]
[Route("API/generate")]
public class ReviewsController : ControllerBase
{
    private IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var review = _reviewService.Generate();
        return Ok(review);
    }
}