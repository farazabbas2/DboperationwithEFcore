using Microsoft.AspNetCore.Mvc;
using DbOperationsWithEfcoreApp.Services.FriendService;
using DbOperationsWithEfcoreApp.Dtos;
using DbOperationsWithEfcoreApp.Hubs;
using DbOperationsWithEfcoreApp.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendController : ControllerBase
    {
        private readonly IFriendService _service;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly AppDbContext _context;

        public FriendController(IFriendService service, IHubContext<NotificationHub> hubContext, AppDbContext context)
        {
            _service = service;
            _hubContext = hubContext;
            _context = context;
        }

        [HttpPost("request")]
        public async Task<IActionResult> SendRequest([FromBody] FriendRequestDto dto)
        {
            var (success, message) = await _service.SendFriendRequest(dto.UserId, dto.FriendId);
            if (!success)
            {
                return BadRequest(new { success = false, message });
            }

            try
            {
                var sender  = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == (int)dto.UserId);
                string senderName = !string.IsNullOrWhiteSpace(sender?.Name)
                    ? sender.Name
                    : (!string.IsNullOrWhiteSpace(sender?.Email) ? sender.Email.Split('@')[0] : $"User #{dto.UserId}");

                await _hubContext.Clients.All.SendAsync("ReceiveFriendRequestAlert", new
                {
                    type = "friend_request",
                    senderId = dto.UserId,
                    recipientId = dto.FriendId,
                    senderName = senderName,
                    message = $"👥 New Friend Request from {senderName}!"
                });
            }
            catch (Exception ex)
            {
                // Ensure notification dispatch failure doesn't break friendship response
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

            try
            {
                var accepter = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == (int)dto.UserId);
                string accepterName = !string.IsNullOrWhiteSpace(accepter?.Name)
                    ? accepter.Name
                    : (!string.IsNullOrWhiteSpace(accepter?.Email) ? accepter.Email.Split('@')[0] : $"User #{dto.UserId}");

                await _hubContext.Clients.All.SendAsync("ReceiveFriendAcceptedAlert", new
                {
                    type = "friend_accepted",
                    senderId = dto.UserId,
                    recipientId = dto.FriendId,
                    senderName = accepterName,
                    message = $"🎉 {accepterName} accepted your friend request!"
                });
            }
            catch (Exception ex)
            {
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
            var otherUserIds = friends.Select(f => f.UserId == userId ? (int)f.FriendId : (int)f.UserId).Distinct().ToList();
            var users = await _context.Users
                .Where(u => otherUserIds.Contains(u.Id))
                .AsNoTracking()
                .ToDictionaryAsync(u => (long)u.Id, u => string.IsNullOrWhiteSpace(u.Name) ? (u.Email.Split('@')[0]) : u.Name);

            var result = friends.Select(f =>
            {
                var otherId = f.UserId == userId ? f.FriendId : f.UserId;
                return new
                {
                    id = f.Id,
                    userId = f.UserId,
                    friendId = f.FriendId,
                    status = f.status,
                    createdAt = f.CreatedAt,
                    friendName = users.ContainsKey(otherId) ? users[otherId] : $"User #{otherId}"
                };
            });

            return Ok(result);
        }

        [HttpGet("pending/{userId}")]
        public async Task<IActionResult> GetPendingRequests(long userId)
        {
            var pending = await _service.GetPendingRequests(userId);
            var senderIds = pending.Select(p => (int)p.UserId).Distinct().ToList();
            var users = await _context.Users
                .Where(u => senderIds.Contains(u.Id))
                .AsNoTracking()
                .ToDictionaryAsync(u => (long)u.Id, u => string.IsNullOrWhiteSpace(u.Name) ? (u.Email.Split('@')[0]) : u.Name);

            var result = pending.Select(p => new
            {
                id = p.Id,
                userId = p.UserId,
                friendId = p.FriendId,
                status = p.status,
                createdAt = p.CreatedAt,
                senderName = users.ContainsKey(p.UserId) ? users[p.UserId] : $"User #{p.UserId}"
            });

            return Ok(result);
        }
    }   
}                                                                                                                                                                                                                                                                                                                    