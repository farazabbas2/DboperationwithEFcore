using Microsoft.AspNetCore.Mvc;
using DbOperationsWithEfcoreApp.Services.FriendService;
using DbOperationsWithEfcoreApp.Dtos;

namespace InventoryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendController : ControllerBase
    {
        private readonly IFriendService _service;
        public FriendController(IFriendService service) { _service = service; }

        [HttpPost("request")]
        public async Task<IActionResult> SendRequest([FromBody] FriendRequestDto dto)
        {
            var (success, message) = await _service.SendFriendRequest(dto.UserId, dto.FriendId);
            if (!success)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message });
        }

        [HttpPost("accept")]
        public async Task<IActionResult> AcceptRequest([FromBody] FriendRequestDto dto)
        {
            var (success, message) = await _service.AcceptFriendRequest(dto.UserId, dto.FriendId);
            if (!success)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message });
        }

        [HttpPost("reject")]
        public async Task<IActionResult> RejectRequest([FromBody] FriendRequestDto dto)
        {
            var (success, message) = await _service.RejectFriendRequest(dto.UserId, dto.FriendId);
            if (!success)
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message });
        }

        [HttpGet("list/{userId}")]
        public async Task<IActionResult> GetFriends(long userId)
        {
            var friends = await _service.GetMyFriends(userId);
            return Ok(friends);
        }

        [HttpGet("pending/{userId}")]
        public async Task<IActionResult> GetPendingRequests(long userId)
        {
            var pending = await _service.GetPendingRequests(userId);
            return Ok(pending);
        }
    }
}