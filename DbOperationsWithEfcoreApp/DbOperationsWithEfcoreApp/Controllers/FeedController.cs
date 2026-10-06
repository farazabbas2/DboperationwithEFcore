using Microsoft.AspNetCore.Mvc;
using DbOperationsWithEfcoreApp.Services.ActivityService;
using DbOperationsWithEfcoreApp.Dtos;

namespace DbOperationsWithEfcoreApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedController : ControllerBase
    {
        private readonly IActivityService _service;
        public FeedController(IActivityService service) { _service = service; }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFeed(long userId)
        {
            var feed = await _service.GetFeedAsync(userId);
            return Ok(feed);
        }

        [HttpPost("activity")]
        public async Task<IActionResult> LogActivity([FromBody] LogActivityDto dto)
        {
            await _service.LogActivity(dto.UserId, dto.ActionType, dto.BookId, dto.Description);
            return Ok(new { success = true, message = "Activity logged successfully" });
        }
    }

   
}