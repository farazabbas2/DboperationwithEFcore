using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Hubs;

namespace DbOperationsWithEfcoreApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiBotController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AiBotController(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("chat")]
        public async Task<ActionResult<AiBotChatResponse>> Chat([FromBody] AiBotChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return Ok(new AiBotChatResponse
                {
                    Reply = "👋 Hello! I am your **BookFlow Studio AI Assistant**. How can I assist you today? You can ask me about books, users, your friendships, system stats, or page navigation!",
                    Type = "text",
                    Data = null
                });
            }

            string rawMessage = request.Message.Trim();
            string msg = rawMessage.ToLowerInvariant();
            string cleanMsg = Regex.Replace(msg, @"[^\w\s]", " ").Trim();
            cleanMsg = Regex.Replace(cleanMsg, @"\s+", " ");

            // 1. Pending friend requests (personal)
            if (Regex.IsMatch(msg, @"\b(pending|incoming|received|outgoing|sent)\b.*\b(request|requests|friend|friends)\b") ||
                Regex.IsMatch(msg, @"\b(friend\s*requests?|pending\s*requests?|pending\s*friends?)\b") ||
                Regex.IsMatch(msg, @"\b(pending\s*request\s*dikhao|meri\s*requests?)\b"))
            {
                return Ok(await HandlePendingRequestsAsync(request.UserId));
            }

            // 2. Personal friends / friend list
            if (Regex.IsMatch(msg, @"\b(my\s+friends?|who\s+are\s+my\s+friends?|show\s+(my\s+)?friends?|list\s+(my\s+)?friends?|friend\s+list)\b") ||
                Regex.IsMatch(msg, @"\b(mere\s+dost|dost\s+ki\s+list|friends?\s+dikhao|kitne\s+dost\s+hai)\b") ||
                Regex.IsMatch(msg, @"\bhow\s+many\s+friends\b"))
            {
                return Ok(await HandleUserFriendsAsync(request.UserId));
            }

            // 3. How friend requests work / Explain friendship feature (English & Hindi)
            if (Regex.IsMatch(msg, @"\bhow\b.*\b(friend|friendship)\b.*\b(work|send|add|accept)\b") ||
                Regex.IsMatch(msg, @"\b(explain|about)\b.*\b(friend|friendship|friend\s*request)\b") ||
                Regex.IsMatch(msg, @"\b(dost\s+kaise\s+(banaye|add)|friend\s+request\s+kaise|request\s+accept\s+kaise)\b") ||
                Regex.IsMatch(msg, @"\b(dost|dosti|social\s*network|social\s*feed|feed\s*kya\s+hai)\b"))
            {
                return Ok(HandleFriendRequestExplanation());
            }

            // 4. How to create books / Admin permissions (English & Hindi)
            if (Regex.IsMatch(msg, @"\bhow\b.*\b(create|add|publish|new)\b.*\bbook\b") ||
                Regex.IsMatch(msg, @"\b(create|add|publish)\b.*\bbook\b") ||
                Regex.IsMatch(msg, @"\b(book\s+kaise\s+(banaye|add|dale|create)|kitab\s+kaise\s+add)\b") ||
                Regex.IsMatch(msg, @"\b(admin\s*only|admin\s*permissions?|admin\s*role)\b"))
            {
                return Ok(HandleCreateBookExplanation());
            }

            // 5. PDF Reading & In-Browser Viewer (English & Hindi)
            if (Regex.IsMatch(msg, @"\b(pdf|pdf\s*reader|how\s+to\s+read|read\s+book|read\s+pdf|upload\s+pdf|view\s+pdf)\b") ||
                Regex.IsMatch(msg, @"\b(kitab\s+kaise\s+padhe|pdf\s+kaise\s+(khole|read|padhe|upload)|pdf\s+kaha\s+hai)\b"))
            {
                return Ok(HandlePdfReadingExplanation());
            }

            // 6. SignalR and real-time notifications (English & Hindi)
            if (Regex.IsMatch(msg, @"\b(signalr|real-?time|websocket|notifications?|push\s*alert|live\s*alert)\b") ||
                Regex.IsMatch(msg, @"\b(notification\s+kaise\s+(aate|kaam|milte)|ghanti\s+icon|live\s+update)\b"))
            {
                return Ok(HandleSignalRExplanation());
            }

            // 7. Roles & Permissions (Admin vs User) (English & Hindi)
            if (Regex.IsMatch(msg, @"\b(roles?|permissions?|admin\s+vs\s+user|access\s+levels?|privileges?)\b") ||
                Regex.IsMatch(msg, @"\b(admin\s+kya\s+kar\s+sakta|user\s+kya\s+kar\s+sakta|role\s+kya\s+hai|fark\s+kya\s+hai|kisko\s+kya\s+permission|admin\s+kaun\s+hai)\b"))
            {
                return Ok(HandleRolesAndPermissions());
            }

            // 8. Tech Stack, Architecture & Database (English & Hindi)
            if (Regex.IsMatch(msg, @"\b(tech\s*stack|technolog(y|ies)|architecture|database|backend|frontend|framework|libraries|stack|sql\s*server|ef\s*core|entity\s*framework|c#|\.net|dotnet|react|vite)\b") ||
                Regex.IsMatch(msg, @"\b(kisme\s+(bana|developed|likha)\s+hai|kaunsi\s+tech|kaun\s+sa\s+(database|db|framework)|backend\s+kisme|frontend\s+kisme|kaunsa\s+server)\b"))
            {
                return Ok(HandleTechStack());
            }

            // 9. All Features & Capabilities (English & Hindi)
            if (Regex.IsMatch(msg, @"\b(features?|functionality|capabilities|modules|what\s+does\s+it\s+offer|all\s+features)\b") ||
                Regex.IsMatch(msg, @"\b(kya\s+features?\s+hai|kya\s+kya\s+(kar\s+sakte|hota|suvidha)\s+hai|features?\s+batao|kaam\s+kya\s+hai)\b"))
            {
                return Ok(HandleProjectFeatures());
            }

            // 10. Authentication & Security (Login, Register, JWT) (English & Hindi)
            if (Regex.IsMatch(msg, @"\b(login|signup|register|forgot\s*password|reset\s*password|authentication|jwt|tokens?)\b") ||
                Regex.IsMatch(msg, @"\b(account\s+kaise\s+banaye|login\s+kaise|signup\s+kaise|password\s+bhul\s+gaye|naya\s+user)\b"))
            {
                return Ok(HandleAuthExplanation());
            }

            // 11. Recent Books / Show Books / List Books
            if (Regex.IsMatch(msg, @"\b(recent|latest|new|newest|top)\b.*\bbooks?\b") ||
                Regex.IsMatch(msg, @"\b(show|list|get|browse|display)\b.*\bbooks?\b") ||
                Regex.IsMatch(msg, @"\b(book\s*list|recommend\s*books?)\b") ||
                Regex.IsMatch(msg, @"\b(books?\s+(dikhao|batao|list|show)|kitabein?\s+dikhao|nayi\s+books?)\b"))
            {
                return Ok(await HandleRecentBooksAsync());
            }

            // 12. Book Count / How Many Books (English & Hindi)
            if (Regex.IsMatch(msg, @"\bhow\s+many\s+books\b") ||
                Regex.IsMatch(msg, @"\b(total\s*books?|book\s*count|number\s+of\s+books)\b") ||
                Regex.IsMatch(msg, @"\b(kitni\s+(books?|kitabein?|kitab)\s+hai|total\s+kitabein?|kitni\s+kitab)\b"))
            {
                return Ok(await HandleBookStatsAsync());
            }

            // 13. Recent Users / Show Users / List Users
            if (Regex.IsMatch(msg, @"\b(recent|latest|new)\b.*\busers?\b") ||
                Regex.IsMatch(msg, @"\b(show|list|get|display)\b.*\busers?\b") ||
                Regex.IsMatch(msg, @"\b(user\s*list|who\s+is\s+registered|users?\s+dikhao)\b"))
            {
                return Ok(await HandleRecentUsersAsync());
            }

            // 14. User Count / How Many Users (English & Hindi)
            if (Regex.IsMatch(msg, @"\bhow\s+many\s+users\b") ||
                Regex.IsMatch(msg, @"\b(total\s*users?|user\s*count|number\s+of\s+users)\b") ||
                Regex.IsMatch(msg, @"\b(kitne\s+(users?|log|members?)\s+hai|total\s+users?\s+kitne)\b"))
            {
                return Ok(await HandleUserStatsAsync());
            }

            // 15. Recent Activity / Activity Feed / Audit
            if (Regex.IsMatch(msg, @"\b(activity|activities|recent\s*activity|activity\s*feed|feed\s*updates?|audit\s*logs?|what's\s*happening)\b") ||
                Regex.IsMatch(msg, @"\b(kya\s+chal\s+raha\s+hai|activity\s+dikhao)\b"))
            {
                return Ok(await HandleRecentActivityAsync());
            }

            // 16. Currencies query
            if (Regex.IsMatch(msg, @"\b(currenc|currencies|exchange\s*rate|paisa|mudra)\b"))
            {
                return Ok(await HandleCurrenciesAsync());
            }

            // 17. Languages query
            if (Regex.IsMatch(msg, @"\b(languag|languages|multilingual|bhasha)\b"))
            {
                return Ok(await HandleLanguagesAsync());
            }

            // 18. Colors query
            if (Regex.IsMatch(msg, @"\b(color|colors|colour|colours|palette|tags?|rang)\b"))
            {
                return Ok(await HandleColorsAsync());
            }

            // 19. Overall System Stats / Analytics
            if (Regex.IsMatch(msg, @"\b(stats|statistics|analytics|metrics|overview|summary|system\s*stats|database\s*stats|db\s*stats)\b") ||
                Regex.IsMatch(msg, @"\b(how\s+many\s+books\s+and\s+users|sab\s+stats\s+dikhao|pura\s+data)\b"))
            {
                return Ok(await HandleSystemStatsAsync());
            }

            // 20. Navigation / Pages / Routes / Menu
            if (Regex.IsMatch(msg, @"\b(navigate|navigation|pages?|routes?|menu|sitemap|where\s+can\s+i\s+go|go\s+to|open)\b") ||
                Regex.IsMatch(msg, @"\b(kaha\s+jaa\s+sakte|pages\s+kaun\s+se|kaha\s+par\s+kya)\b") ||
                Regex.IsMatch(msg, @"\b(dashboard|feed|books\s*page|friends\s*page|currencies\s*page|languages\s*page|colors\s*page)\b"))
            {
                return Ok(HandleNavigation(msg));
            }

            // 21. Complete Project Overview / "About Project" (ANY query asking about the project in English/Hindi/Hinglish!)
            if (Regex.IsMatch(msg, @"\b(what\s+is\s+(this\s+)?(project|app|application|website|system|bookflow)|tell\s+me\s+about\s+(this\s+)?(project|app|website|bookflow)|explain\s+(this\s+)?(project|app|system)|about\s+(the\s+)?(project|app|system|bookflow))\b") ||
                Regex.IsMatch(msg, @"\b(project\s+(overview|description|details|info|information|purpose|summary|intro|introduction))\b") ||
                Regex.IsMatch(msg, @"\b(why\s+was\s+this\s+built|what\s+does\s+this\s+(app|project|website)\s+do|what\s+can\s+i\s+do\s+here)\b") ||
                Regex.IsMatch(msg, @"\b(ye|yeh|is|iss)\s+(project|app|website|system)\s+(kya|kis|kaise|kyu)\b") ||
                Regex.IsMatch(msg, @"\b(project|app|website|system)\s+k[eai]\s+ba?a?re?\s+m[eai]n?\b") ||
                Regex.IsMatch(msg, @"\b(batao|samjhao|bataiye|batao\s+na)\s+(ye\s+|yeh\s+|is\s+)?(project|app|website)\b") ||
                Regex.IsMatch(msg, @"\b(ye|yeh)\s+kya\s+(karta|cheez|hai)\b") ||
                Regex.IsMatch(msg, @"\b(kiske\s+liye\s+(hai|banaya)|kis\s+cheez\s+ka\s+(project|app))\b") ||
                Regex.IsMatch(msg, @"\b(isme|ismein|iss\s+me)\s+kya\s+(hota|hai|kar\s+sakte)\b") ||
                Regex.IsMatch(msg, @"\b(project\s+(kya|kaise|hai|batao|samjhao))\b") ||
                Regex.IsMatch(msg, @"\b(kya\s+hai\s+(ye|yeh|is|project|app))\b") ||
                msg == "project" || msg == "about" || msg == "app" || msg == "bookflow" || msg == "overview" ||
                cleanMsg == "project" || cleanMsg == "about" || cleanMsg == "bookflow" || cleanMsg == "ye project kya hai" || cleanMsg == "yeh project kya hai")
            {
                return Ok(HandleAboutProject());
            }

            // 22. Help / Capabilities
            if (Regex.IsMatch(msg, @"\b(help|what\s+can\s+you\s+do|commands?|guide|how\s+to\s+use|capabilities|madad)\b"))
            {
                return Ok(HandleHelp());
            }

            // 23. Greetings & Salutations (English & Hindi)
            if (Regex.IsMatch(msg, @"\b(hello|hi|hey|greetings|good\s+morning|good\s+afternoon|good\s+evening|howdy|sup|yo|namaste|kaise\s+ho)\b"))
            {
                return Ok(HandleGreeting());
            }

            // 24. Gratitude / Goodbyes
            if (Regex.IsMatch(msg, @"\b(thanks?|thank\s+you|bye|goodbye|see\s+ya|shukriya|dhanyawad)\b"))
            {
                return Ok(new AiBotChatResponse
                {
                    Reply = "🌟 You're very welcome (Bahut bahut shukriya)! If you need anything else regarding BookFlow Studio books, friends, or platform stats, just ask. Happy reading! 📚✨",
                    Type = "text",
                    Data = null
                });
            }

            // 25. Fallback / Unknown queries — provides complete project context
            return Ok(HandleFallback(rawMessage));
        }

        #region Personal Friend & Pending Request Handlers

        private async Task<AiBotChatResponse> HandlePendingRequestsAsync(int? userId)
        {
            if (!userId.HasValue || userId.Value <= 0)
            {
                return new AiBotChatResponse
                {
                    Reply = "🔒 **Authentication Context Required**: Please make sure you are logged in so I can look up your pending friend requests. Once logged in, your user ID lets me query incoming and outgoing invitations!",
                    Type = "text",
                    Data = null
                };
            }

            int uid = userId.Value;

            // Incoming requests: status == 0 and FriendId == current user
            var incomingFriendships = await _context.Friendships
                .AsNoTracking()
                .Where(f => f.FriendId == uid && f.status == 0)
                .ToListAsync();

            // Outgoing requests: status == 0 and UserId == current user
            var outgoingFriendships = await _context.Friendships
                .AsNoTracking()
                .Where(f => f.UserId == uid && f.status == 0)
                .ToListAsync();

            var incomingSenderIds = incomingFriendships.Select(f => f.UserId).ToList();
            var outgoingReceiverIds = outgoingFriendships.Select(f => f.FriendId).ToList();

            var incomingSenders = await _context.Users
                .AsNoTracking()
                .Where(u => incomingSenderIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Name, u.Email })
                .ToListAsync();

            var outgoingReceivers = await _context.Users
                .AsNoTracking()
                .Where(u => outgoingReceiverIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Name, u.Email })
                .ToListAsync();

            string reply;
            if (incomingFriendships.Count == 0 && outgoingFriendships.Count == 0)
            {
                reply = "📬 **Friend Requests**: You have **0 pending incoming requests** and **0 outgoing requests**.\n\n" +
                        "💡 *Tip: Visit the Friends page (`/friends`) to discover fellow readers and send friend requests!*";
            }
            else
            {
                reply = $"📬 **Pending Friend Requests Status**:\n\n" +
                        $"• **Incoming Requests ({incomingFriendships.Count})**:\n" +
                        (incomingSenders.Count > 0
                            ? string.Join("\n", incomingSenders.Select(s => $"  - 👤 **{s.Name}** ({s.Email}) sent you a request."))
                            : "  *(No incoming requests)*") +
                        $"\n\n• **Outgoing Requests ({outgoingFriendships.Count})**:\n" +
                        (outgoingReceivers.Count > 0
                            ? string.Join("\n", outgoingReceivers.Select(r => $"  - ⏳ Awaiting response from **{r.Name}** ({r.Email})."))
                            : "  *(No outgoing requests pending)*");
            }

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "list",
                Data = new
                {
                    incomingCount = incomingFriendships.Count,
                    outgoingCount = outgoingFriendships.Count,
                    incoming = incomingSenders,
                    outgoing = outgoingReceivers
                }
            };
        }

        private async Task<AiBotChatResponse> HandleUserFriendsAsync(int? userId)
        {
            if (!userId.HasValue || userId.Value <= 0)
            {
                return new AiBotChatResponse
                {
                    Reply = "🔒 **Authentication Context Required**: Please log in so I can show your personal friends list! Without a user context, I cannot retrieve your private friendships.",
                    Type = "text",
                    Data = null
                };
            }

            int uid = userId.Value;

            // Accepted friendships: status == 1 where user is either sender (UserId) or receiver (FriendId)
            var acceptedFriendships = await _context.Friendships
                .AsNoTracking()
                .Where(f => f.status == 1 && (f.UserId == uid || f.FriendId == uid))
                .ToListAsync();

            var friendIds = acceptedFriendships
                .Select(f => f.UserId == uid ? f.FriendId : f.UserId)
                .Distinct()
                .ToList();

            var friends = await _context.Users
                .AsNoTracking()
                .Where(u => friendIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Role,
                    u.isActive
                })
                .ToListAsync();

            string reply;
            if (friends.Count == 0)
            {
                reply = "🤝 **Your Friends**: You don't have any accepted friends yet.\n\n" +
                        "💡 Head over to `/friends` to search for other members and send your first request! Once accepted, you'll see their activity on the live feed.";
            }
            else
            {
                reply = $"🤝 **Your Friends ({friends.Count})**:\n\n" +
                        string.Join("\n", friends.Select(f => $"• 👤 **{f.Name}** ({f.Email}) - Role: `{f.Role}` {(f.isActive ? "🟢 Active" : "⚪ Inactive")}"));
            }

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "list",
                Data = new
                {
                    totalFriends = friends.Count,
                    friends = friends
                }
            };
        }

        #endregion

        #region Database Stats Handlers

        private async Task<AiBotChatResponse> HandleSystemStatsAsync()
        {
            var totalBooks = await _context.Books.CountAsync();
            var activeBooks = await _context.Books.CountAsync(b => b.IsActive);
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.isActive);
            var totalFriendships = await _context.Friendships.CountAsync(f => f.status == 1);
            var pendingFriendships = await _context.Friendships.CountAsync(f => f.status == 0);
            var totalActivities = await _context.UserActivities.CountAsync();
            var totalCurrencies = await _context.Currency.CountAsync();
            var totalLanguages = await _context.Languages.CountAsync();
            var totalColors = await _context.Colors.CountAsync();
            var totalAuditLogs = await _context.AuditLogs.CountAsync();

            string reply = $"📊 **BookFlow Studio — System Overview**\n\n" +
                           $"• 📚 **Books**: {totalBooks} total ({activeBooks} active)\n" +
                           $"• 👥 **Users**: {totalUsers} registered ({activeUsers} active)\n" +
                           $"• 🤝 **Friendships**: {totalFriendships} active connections ({pendingFriendships} pending requests)\n" +
                           $"• ⚡ **User Activities**: {totalActivities} recorded events\n" +
                           $"• 💱 **Currencies**: {totalCurrencies} configured\n" +
                           $"• 🌐 **Languages**: {totalLanguages} available\n" +
                           $"• 🎨 **Color Themes**: {totalColors} palettes\n" +
                           $"• 🛡️ **Audit Logs**: {totalAuditLogs} system records\n\n" +
                           $"💡 *All data is synchronized with the live SQL Server (`BookDb`).*";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "stats",
                Data = new
                {
                    totalBooks,
                    activeBooks,
                    totalUsers,
                    activeUsers,
                    totalFriendships,
                    pendingFriendships,
                    totalActivities,
                    totalCurrencies,
                    totalLanguages,
                    totalColors,
                    totalAuditLogs
                }
            };
        }

        private async Task<AiBotChatResponse> HandleBookStatsAsync()
        {
            var totalBooks = await _context.Books.CountAsync();
            var activeBooks = await _context.Books.CountAsync(b => b.IsActive);
            var inactiveBooks = totalBooks - activeBooks;

            string reply = $"📚 **Book Catalog Statistics**:\n\n" +
                           $"• **Total Books**: **{totalBooks}**\n" +
                           $"• **Active / Published**: **{activeBooks}**\n" +
                           $"• **Draft / Inactive**: **{inactiveBooks}**\n\n" +
                           $"Would you like me to show the latest additions? Ask me for *'recent books'* or navigate to `/books`!";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "stats",
                Data = new
                {
                    totalBooks,
                    activeBooks,
                    inactiveBooks
                }
            };
        }

        private async Task<AiBotChatResponse> HandleRecentBooksAsync()
        {
            var recentBooks = await _context.Books
                .AsNoTracking()
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Description,
                    b.NoOfPages,
                    b.IsActive,
                    b.CreatedAt
                })
                .ToListAsync();

            if (recentBooks.Count == 0)
            {
                return new AiBotChatResponse
                {
                    Reply = "📚 No books found in the database yet. Administrators can add books via the `/books` page!",
                    Type = "list",
                    Data = recentBooks
                };
            }

            string reply = $"📚 **Latest Books in BookFlow Studio (Top 5)**:\n\n" +
                           string.Join("\n\n", recentBooks.Select((b, idx) =>
                               $"{idx + 1}. **{b.Title}** (ID: {b.Id})\n" +
                               $"   📄 {b.NoOfPages} pages | Status: {(b.IsActive ? "🟢 Active" : "⚪ Inactive")}\n" +
                               $"   📝 {(string.IsNullOrWhiteSpace(b.Description) ? "No description provided." : (b.Description.Length > 80 ? b.Description.Substring(0, 77) + "..." : b.Description))}\n" +
                               $"   📅 Added: {b.CreatedAt:yyyy-MM-dd}")) +
                           $"\n\n💡 *View the full catalog at `/books`.*";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "list",
                Data = recentBooks
            };
        }

        private async Task<AiBotChatResponse> HandleUserStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.isActive);
            var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
            var regularUsers = totalUsers - adminCount;

            string reply = $"👥 **Community & User Stats**:\n\n" +
                           $"• **Total Users**: **{totalUsers}**\n" +
                           $"• **Active Members**: **{activeUsers}**\n" +
                           $"• **Administrators**: **{adminCount}**\n" +
                           $"• **Standard Members**: **{regularUsers}**\n\n" +
                           $"You can ask me to *'show users'* to see recent members or visit `/friends` to connect!";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "stats",
                Data = new
                {
                    totalUsers,
                    activeUsers,
                    adminCount,
                    regularUsers
                }
            };
        }

        private async Task<AiBotChatResponse> HandleRecentUsersAsync()
        {
            var recentUsers = await _context.Users
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Role,
                    u.isActive,
                    u.CreatedAt
                })
                .ToListAsync();

            if (recentUsers.Count == 0)
            {
                return new AiBotChatResponse
                {
                    Reply = "👥 No registered users found in the database.",
                    Type = "list",
                    Data = recentUsers
                };
            }

            string reply = $"👥 **Recently Registered Members (Top 5)**:\n\n" +
                           string.Join("\n", recentUsers.Select((u, idx) =>
                               $"{idx + 1}. 👤 **{u.Name}** ({u.Email})\n" +
                               $"   Role: `{u.Role}` | Status: {(u.isActive ? "🟢 Active" : "⚪ Inactive")} | Joined: {u.CreatedAt:yyyy-MM-dd}")) +
                           $"\n\n💡 *Want to connect? Send them a friend request on the `/friends` page!*";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "list",
                Data = recentUsers
            };
        }

        private async Task<AiBotChatResponse> HandleRecentActivityAsync()
        {
            var activities = await (from a in _context.UserActivities.AsNoTracking()
                                    join u in _context.Users.AsNoTracking() on a.UserId equals u.Id into userGroup
                                    from u in userGroup.DefaultIfEmpty()
                                    orderby a.CreatedAt descending
                                    select new
                                    {
                                        a.Id,
                                        a.UserId,
                                        UserName = u != null ? u.Name : "Community Member",
                                        a.ActionType,
                                        a.BookId,
                                        a.Description,
                                        a.CreatedAt
                                    }).Take(5).ToListAsync();

            if (activities.Count == 0)
            {
                return new AiBotChatResponse
                {
                    Reply = "⚡ **Activity Feed**: No activities logged yet. As members browse, add friends, or interact with books, updates will appear in real time on `/feed`!",
                    Type = "list",
                    Data = activities
                };
            }

            string reply = $"⚡ **Latest Activity Feed Updates (Top 5)**:\n\n" +
                           string.Join("\n\n", activities.Select((act, idx) =>
                               $"{idx + 1}. 📌 **{act.ActionType}** by **{act.UserName}**\n" +
                               $"   📝 {act.Description}\n" +
                               $"   🕒 {act.CreatedAt:yyyy-MM-dd HH:mm:ss}")) +
                           $"\n\n💡 *Follow live updates in real time at `/feed`.*";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "list",
                Data = activities
            };
        }

        private async Task<AiBotChatResponse> HandleCurrenciesAsync()
        {
            var total = await _context.Currency.CountAsync();
            var list = await _context.Currency.AsNoTracking().Take(10).ToListAsync();

            string reply = $"💱 **Currencies Master**:\n\n" +
                           $"• We have **{total}** currency configuration(s) available in the system.\n" +
                           $"• Currencies enable multi-currency pricing for books (`BookPrices`).\n\n" +
                           $"🧭 Visit `/currencies` to view and manage available currency definitions.";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "list",
                Data = new { totalCurrencies = total, currencies = list }
            };
        }

        private async Task<AiBotChatResponse> HandleLanguagesAsync()
        {
            var total = await _context.Languages.CountAsync();
            var list = await _context.Languages.AsNoTracking().Take(10).ToListAsync();

            string reply = $"🌐 **Languages Master**:\n\n" +
                           $"• We have **{total}** publishing language(s) configured in BookFlow Studio.\n" +
                           $"• Each book can be mapped to multiple languages via `BookLanguages`.\n\n" +
                           $"🧭 Visit `/languages` to browse all registered languages.";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "list",
                Data = new { totalLanguages = total, languages = list }
            };
        }

        private async Task<AiBotChatResponse> HandleColorsAsync()
        {
            var total = await _context.Colors.CountAsync();
            var list = await _context.Colors.AsNoTracking().Take(10).ToListAsync();

            string reply = $"🎨 **Colors & Cover Themes Master**:\n\n" +
                           $"• There are **{total}** color tag(s) defined in the system.\n" +
                           $"• Colors are used to categorize and visually customize books via `BookColors`.\n\n" +
                           $"🧭 Visit `/colors` to view all color palettes.";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "list",
                Data = new { totalColors = total, colors = list }
            };
        }

        #endregion

        #region Knowledge Base & Explanation Handlers

        private AiBotChatResponse HandleFriendRequestExplanation()
        {
            string reply = "🤝 **How Friend Requests Work in BookFlow Studio**:\n\n" +
                           "1. **Find Readers**: Head over to the **Friends** page (`/friends`) or explore member profiles.\n" +
                           "2. **Send Invitation**: Click **Add Friend**. A new friendship record is generated with `status = 0` (Pending).\n" +
                           "3. **Real-Time Push Notification**: The recipient immediately receives an alert through our **SignalR** websocket connection (`NotificationHub`).\n" +
                           "4. **Accept / Decline**: The recipient can review pending requests at `/friends`. Upon acceptance, `status` changes to `1` (Accepted).\n" +
                           "5. **Connected Feed**: Mutual friends can view each other's reading milestones and interactions on the **Activity Feed** (`/feed`)!\n\n" +
                           "💡 *Want to check your pending requests right now? Ask me: 'What are my pending friend requests?'*";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "text",
                Data = null
            };
        }

        private AiBotChatResponse HandleCreateBookExplanation()
        {
            string reply = "📖 **How to Create Books (Admin Only)**:\n\n" +
                           "• **Permissions**: Book creation is restricted to users with the **`Admin`** role to maintain catalog quality.\n" +
                           "• **Book Details**: An Admin can specify:\n" +
                           "  - 🏷️ **Title** & **Description**\n" +
                           "  - 📄 **Number of Pages** (`NoOfPages`)\n" +
                           "  - 🎨 **Color Themes** (associated via `BookColors`)\n" +
                           "  - 🌐 **Publishing Languages** (associated via `BookLanguages`)\n" +
                           "  - 💰 **Pricing** across multiple currencies (associated via `BookPrices`)\n" +
                           "• **Real-Time Broadcast**: Once created, a SignalR broadcast notifies active users of the new catalog item!\n\n" +
                           "🧭 To create or manage books, sign in as an Admin and navigate to `/books`.";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "text",
                Data = null
            };
        }

        private AiBotChatResponse HandleSignalRExplanation()
        {
            string reply = "⚡ **SignalR Real-Time Notifications in BookFlow Studio**:\n\n" +
                           "• **Hub Endpoint**: Connected via ASP.NET Core SignalR at `/hubs/notification` (`NotificationHub`).\n" +
                           "• **Instant Push Events**:\n" +
                           "  - 📬 **Friend Requests**: Immediate notification when someone sends or accepts an invitation.\n" +
                           "  - 📚 **Book Catalog Updates**: Instant banner notification when an Admin publishes a new book.\n" +
                           "  - 📰 **Activity Feed**: Live stream updates without needing manual page refreshes.\n" +
                           "• **Protocol**: Full duplex WebSockets with automatic fallback to Server-Sent Events (SSE) and Long Polling.";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "text",
                Data = null
            };
        }

        private AiBotChatResponse HandleNavigation(string msg)
        {
            var routes = new[]
            {
                new { Path = "/dashboard", Name = "Dashboard", Description = "High-level analytics, key performance indicators, and quick summary cards." },
                new { Path = "/books", Name = "Books Catalog", Description = "Browse, search, and view detailed book records with prices, languages, and colors." },
                new { Path = "/feed", Name = "Activity Feed", Description = "Real-time stream of community events, reader interactions, and updates." },
                new { Path = "/friends", Name = "Friends & Network", Description = "Discover users, send invitations, accept requests, and manage your friends." },
                new { Path = "/currencies", Name = "Currency Master", Description = "View and manage pricing currencies supported across book stores." },
                new { Path = "/languages", Name = "Language Master", Description = "Configure and manage languages for multilingual book listings." },
                new { Path = "/colors", Name = "Color Themes", Description = "Manage cover color palettes and categorization badges." }
            };

            string reply;
            if (msg.Contains("dashboard"))
            {
                reply = "🧭 **Navigation — Dashboard (`/dashboard`)**:\n\n" +
                        "The Dashboard gives you an executive overview of your library stats, top activities, and quick access shortcuts.";
            }
            else if (msg.Contains("feed"))
            {
                reply = "🧭 **Navigation — Activity Feed (`/feed`)**:\n\n" +
                        "The Activity Feed provides real-time community engagement logs powered by SignalR.";
            }
            else if (msg.Contains("book"))
            {
                reply = "🧭 **Navigation — Books Catalog (`/books`)**:\n\n" +
                        "Explore the full collection of books, read descriptions, and filter by language, currency, or color.";
            }
            else if (msg.Contains("friend"))
            {
                reply = "🧭 **Navigation — Friends (`/friends`)**:\n\n" +
                        "Connect with readers, review pending invitations, and manage your social network.";
            }
            else if (msg.Contains("currenc"))
            {
                reply = "🧭 **Navigation — Currencies (`/currencies`)**:\n\n" +
                        "Manage multi-currency pricing configurations for your book catalog.";
            }
            else if (msg.Contains("languag"))
            {
                reply = "🧭 **Navigation — Languages (`/languages`)**:\n\n" +
                        "Configure supported languages for internationalized book listings.";
            }
            else if (msg.Contains("color"))
            {
                reply = "🧭 **Navigation — Colors (`/colors`)**:\n\n" +
                        "Customize and organize book cover color palettes.";
            }
            else
            {
                reply = "🧭 **BookFlow Studio — Available Navigation Routes**:\n\n" +
                        string.Join("\n", routes.Select(r => $"• 🔗 **{r.Name}** (`{r.Path}`)\n  *{r.Description}*")) +
                        "\n\n💡 *Where would you like to go?*";
            }

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "navigation",
                Data = routes
            };
        }

        private AiBotChatResponse HandleAboutProject()
        {
            string reply = "🌟 **BookFlow Studio — Complete Project Overview (प्रोजेक्ट का परिचय)**\n\n" +
                           "**BookFlow Studio** ek comprehensive **Digital Library Management & Social Reader Platform** hai jo digital books ke reading/cataloging aur readers ke community engagement ke liye banaya gaya hai.\n\n" +
                           "🎯 **Main Purpose (मुख्य उद्देश्य)**:\n" +
                           "• Readers ko digital books khojne, multi-language/multi-currency pricing dekhne, aur browser ke andar **PDF format** me direct padhne ki suvidha dena.\n" +
                           "• Readers ke beech **social networking** establish karna — taaki log ek-dusre ko friend bana sakein, live friend requests bhej sakein, aur real-time reading updates dekh sakein.\n\n" +
                           "🚀 **Key Modules (प्रमुख फीचर्स)**:\n" +
                           "1. 📚 **Digital Library (`/books`)**: Books catalog, page counts, multi-language support, multi-currency prices, cover colors, aur embedded PDF reader (`pdfjs-dist`).\n" +
                           "2. 👥 **Social Network (`/friends`)**: Readers ko search karna, real-time friend requests bhejna, accept/reject karna, aur friends list.\n" +
                           "3. ⚡ **SignalR Real-Time Alerts**: WebSocket-powered live push notifications jab friend request aaye ya nayi book publish ho (`NotificationHub`).\n" +
                           "4. 📰 **Live Activity Feed (`/feed`)**: Real-time reader activity feed aur backend audit logs (`AuditLogs`).\n" +
                           "5. 📊 **Executive Dashboard (`/dashboard`)**: Total books, registered members, quick action cards, aur platform metrics.\n" +
                           "6. 💱 **Configuration Masters**: Currency Master (`/currencies`), Language Master (`/languages`), Color Master (`/colors`).\n" +
                           "7. 🛡️ **Role Security**: Secure JWT authentication with **Admin** (Catalog Creator) and **User** (Reader).\n\n" +
                           "💻 **Tech Stack Overview**:\n" +
                           "• **Backend**: ASP.NET Core 8 Web API + Entity Framework Core (SQL Server `BookDb` on `ERAIT1104`).\n" +
                           "• **Frontend**: React 19 + Vite 8 + React Router v7 + SignalR Client + PDF.js Canvas Viewer.\n\n" +
                           "💡 *Aap mujhse kisi bhi topic par detail me pooch sakte hain, jaise: 'Features', 'Tech Stack', 'Admin vs User', 'Kitni books hai', ya 'Dost kaise banaye'!*";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "text",
                Data = new
                {
                    projectName = "BookFlow Studio",
                    architecture = "Full-Stack Web API + SPA",
                    backend = "ASP.NET Core 8 Web API with EF Core",
                    database = "SQL Server (BookDb on ERAIT1104)",
                    frontend = "React 19 + Vite 8",
                    realTime = "SignalR WebSockets (NotificationHub)",
                    auth = "JWT Bearer Tokens"
                }
            };
        }

        private AiBotChatResponse HandleProjectFeatures()
        {
            string reply = "🚀 **BookFlow Studio — All Features & Capabilities (सभी फीचर्स)**:\n\n" +
                           "1. 📚 **Advanced Digital Library (`/books`)**:\n" +
                           "   • Digital books browse, search aur real-time filtering.\n" +
                           "   • Multi-currency pricing (`BookPrices`) alag-alag desh ki mudraon ke liye.\n" +
                           "   • Multilingual publishing (`BookLanguages`) multiple bhashaon ke liye.\n" +
                           "   • Cover badge color themes (`BookColors`).\n" +
                           "   • Embedded in-browser PDF reader (`pdfjs-dist`) bina kisi external software ke.\n" +
                           "   • Batch JSON bulk book creation (`/books/bulk`) Admins ke liye.\n\n" +
                           "2. 🤝 **Social Friendship System (`/friends`)**:\n" +
                           "   • Community members ko search karna.\n" +
                           "   • Friend request bhejna (Status 0 = Pending).\n" +
                           "   • SignalR dwara instant real-time popup notification.\n" +
                           "   • Request accept ya reject karna (Status 1 = Accepted).\n\n" +
                           "3. ⚡ **SignalR Real-Time Engine (`NotificationHub`)**:\n" +
                           "   • Live WebSocket connection background me chalta hai.\n" +
                           "   • Friend request notifications aur new book announcements turant milte hain bina page refresh kiye!\n\n" +
                           "4. 📰 **Live Activity Feed (`/feed`)**:\n" +
                           "   • Sabhi members ke reading activities ka chronological stream.\n" +
                           "   • Backend SQL Server me har activity ka audit logging.\n\n" +
                           "5. 📊 **Analytics Dashboard (`/dashboard`)**:\n" +
                           "   • Live stats: Total books, active members, connection counts.\n" +
                           "   • Quick action links aur interactive cards.\n\n" +
                           "6. 🔍 **Global Spotlight Search (Ctrl + K)**:\n" +
                           "   • Kisi bhi page se instant Command Palette khol kar books, currencies, ya pages search karein.\n\n" +
                           "7. 🤖 **Context-Aware AI Assistant (Chatbot)**:\n" +
                           "   • Real database query karke live counts batata hai aur poore project ki jankari deta hai!";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "text",
                Data = null
            };
        }

        private AiBotChatResponse HandleTechStack()
        {
            string reply = "💻 **BookFlow Studio — Complete Tech Stack & Architecture**:\n\n" +
                           "🌐 **Backend Architecture**:\n" +
                           "• **Framework**: ASP.NET Core 8 Web API (.NET 8.0 / C# 12)\n" +
                           "• **ORM**: Entity Framework Core with LINQ to Entities\n" +
                           "• **Database**: Microsoft SQL Server (`BookDb` on `ERAIT1104` instance)\n" +
                           "• **Real-Time Hub**: ASP.NET Core SignalR (`NotificationHub` at `/hubs/notification` & `/notificationHub`)\n" +
                           "• **Authentication**: JWT Bearer Tokens with Role Claims (`Admin`, `User`)\n" +
                           "• **Password Security**: BCrypt password hashing\n" +
                           "• **Audit Trail**: Entity state tracking via `AuditLogs` table\n\n" +
                           "🎨 **Frontend Architecture**:\n" +
                           "• **Framework**: React 19.2 + Vite 8.3 build toolchain\n" +
                           "• **Routing**: React Router v7 with Protected & Admin route guards\n" +
                           "• **Real-Time Client**: `@microsoft/signalr` with automatic reconnection\n" +
                           "• **PDF Engine**: `pdfjs-dist` embedded canvas rendering\n" +
                           "• **HTTP Client**: Axios with global Authorization Bearer interceptor\n" +
                           "• **UI/UX**: Modern dark-themed glassmorphism styling with responsive mobile drawer\n\n" +
                           "🔌 **API Integration**:\n" +
                           "• RESTful API endpoints under `/api/[controller]`\n" +
                           "• Frontend dev proxy forwards `/api` and `/notificationHub` to `https://localhost:7103`!";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "text",
                Data = new
                {
                    backend = "ASP.NET Core 8 Web API",
                    orm = "Entity Framework Core",
                    database = "Microsoft SQL Server (BookDb)",
                    realtime = "SignalR WebSockets",
                    frontend = "React 19 + Vite 8",
                    pdfViewer = "pdfjs-dist",
                    auth = "JWT Bearer Tokens"
                }
            };
        }

        private AiBotChatResponse HandleRolesAndPermissions()
        {
            string reply = "👥 **User Roles & Permissions in BookFlow Studio (रोल्स और अधिकार)**:\n\n" +
                           "System me do mukhya roles hote hain:\n\n" +
                           "👑 **1. Admin Role (एडमिन)**:\n" +
                           "• **Catalog Management**: Nayi books create karna, update karna, delete karna (`/books/create`, `/books/bulk`).\n" +
                           "• **PDF Upload**: Books ke sath digital PDF files attach karna (`/Book/{id}/upload-pdf`).\n" +
                           "• **Master Configuration**: Currencies, Languages aur Colors create aur modify karna (`/currencies`, `/languages`, `/colors`).\n" +
                           "• **Audit Trail**: System audit logs inspect karna.\n\n" +
                           "👤 **2. Regular User Role (रीडर / सामान्य यूजर)**:\n" +
                           "• **Browse & Discover**: Puri digital library me books search aur filter karna (`/books`).\n" +
                           "• **PDF Reading**: Book details page par jaakar in-browser PDF direct padhna.\n" +
                           "• **Social Connections**: Dusre readers ko search karke Friend Request bhejna aur accept/reject karna (`/friends`).\n" +
                           "• **Personal Feed & Dashboard**: Apne reading updates aur friends ki activity live feed par dekhna (`/feed`, `/dashboard`).\n\n" +
                           "🔒 *Backend API endpoints par `[Authorize(Roles = \"Admin\")]` dwara Admin permissions enforce ki gayi hain.*";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "text",
                Data = null
            };
        }

        private AiBotChatResponse HandlePdfReadingExplanation()
        {
            string reply = "📖 **In-Browser PDF Reading (पीडीएफ कैसे पढ़ें)**:\n\n" +
                           "• **Embedded Reader**: BookFlow Studio me `pdfjs-dist` integrated hai jo bina kisi external PDF app ke direct browser canvas me PDF render karta hai.\n\n" +
                           "📖 **Kaise Padhein (Steps to Read)**:\n" +
                           "1. Top menu se **Books** (`/books`) page par jayein.\n" +
                           "2. Apni pasand ki kisi bhi book par click karke uski details open karein (`/books/:id`).\n" +
                           "3. Wahan **Read Book / Open PDF** button par click karein.\n" +
                           "4. In-browser viewer open hoga jisme Previous/Next page navigation aur zoom controls available hain!\n\n" +
                           "💡 *Note: Admins book create karte waqt ya update karte waqt PDF document upload kar sakte hain.*";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "text",
                Data = null
            };
        }

        private AiBotChatResponse HandleAuthExplanation()
        {
            string reply = "🔐 **Authentication & Security (लॉगिन और सुरक्षा)**:\n\n" +
                           "• **Login & Registration**: Users `/login` aur `/signup` pages se secure account bana aur login kar sakte hain.\n" +
                           "• **Password Protection**: Passwords backend me `BCrypt` hashing se encrypt rehte hain.\n" +
                           "• **JWT Tokens**: Login hone par server ek **JSON Web Token (JWT)** bhejta hai jo `localStorage` me save hota hai.\n" +
                           "• **Automatic Authorization**: Frontend ka Axios interceptor har API request me `Bearer <token>` attach karta hai.\n" +
                           "• **Password Recovery**: Password bhulne par `/forgot-password` aur `/reset-password` se reset kiya ja sakta hai.";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "text",
                Data = null
            };
        }

        private AiBotChatResponse HandleHelp()
        {
            string reply = "🤖 **BookFlow Studio AI Assistant — What I Can Do (मैं क्या बता सकता हूँ)**:\n\n" +
                           "Aap mujhse is project ke kisi bhi vishay par pooch sakte hain:\n\n" +
                           "📖 **Project & Architecture (प्रोजेक्ट और तकनीक)**:\n" +
                           "• *'Ye project kya hai?'* (Project overview & purpose)\n" +
                           "• *'Kya features hai is app me?'* (All capabilities & modules)\n" +
                           "• *'Tech stack aur database kya hai?'* (.NET 8, React, SQL Server, SignalR)\n" +
                           "• *'Admin aur regular user me kya fark hai?'* (Roles & permissions)\n\n" +
                           "📊 **Live Data & Statistics (लाइव आंकड़े)**:\n" +
                           "• *'Kitni books hai?'* ya *'Kitne users registered hai?'*\n" +
                           "• *'Recent books dikhao'* (Top 5 latest books)\n" +
                           "• *'System statistics dikhao'* (Overall database metrics)\n" +
                           "• *'Recent activity kya hai?'*\n\n" +
                           "🤝 **Friends & Social (दोस्त और सोशल)**:\n" +
                           "• *'Dost kaise banaye?'* (How friend requests work)\n" +
                           "• *'Mere dost dikhao'* ya *'Pending friend requests'* (Personal friends data)\n\n" +
                           "🧭 **Navigation (रास्ते और पेज)**:\n" +
                           "• *'Kaha jaa sakte hai?'* ya *'Dashboard / Feed / Books page samjhao'*\n\n" +
                           "✨ Try typing any question above or tap a quick action button!";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "text",
                Data = null
            };
        }

        private AiBotChatResponse HandleGreeting()
        {
            string reply = "👋 **Namaste aur Welcome to BookFlow Studio!** 📚\n\n" +
                           "Main aapka intelligent AI Assistant hoon. Main is poore project ke baare me (features, books, friends, statistics, database, tech stack) sab kuch janta hoon aur real-time database data bhi bata sakta hoon.\n\n" +
                           "✨ Aap mujhse pooch sakte hain:\n" +
                           "• *'Ye project kya hai?'*\n" +
                           "• *'Kya features hai is app me?'*\n" +
                           "• *'Tech stack kya hai?'*\n" +
                           "• *'Kitni books hai database me?'*\n\n" +
                           "Boliye, main aapki kya madad karoon? 🚀";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "text",
                Data = null
            };
        }

        private AiBotChatResponse HandleFallback(string rawMessage)
        {
            string reply = $"👋 Main **BookFlow Studio AI Assistant** hoon! Aapne poocha: *\"{rawMessage}\"*\n\n" +
                           "Main is project ke har pehlu ke baare me janta hoon. Yahan kuch popular topics hain jo aap pooch sakte hain:\n\n" +
                           "📖 **Project Info**:\n" +
                           "• *'Ye project kya hai?'* (Complete project overview & purpose)\n" +
                           "• *'Kya features hai is app me?'* (All capabilities & modules)\n" +
                           "• *'Tech stack aur database kya hai?'* (.NET 8, React, SQL Server, SignalR)\n" +
                           "• *'Admin aur regular user me kya fark hai?'* (Roles & permissions)\n\n" +
                           "📊 **Live Data & Stats**:\n" +
                           "• *'Kitni books hai?'* ya *'Kitne users registered hai?'*\n" +
                           "• *'System statistics dikhao'* (Real-time DB metrics)\n" +
                           "• *'Recent activity kya hai?'*\n\n" +
                           "🤝 **Friends & Navigation**:\n" +
                           "• *'Dost kaise banaye?'* ya *'Pending requests dikhao'*\n" +
                           "• *'Pages aur routes samjhao'*\n\n" +
                           "✨ Aap inme se koi bhi question pooch sakte hain ya quick action buttons par click kar sakte hain!";

            return new AiBotChatResponse
            {
                Reply = reply,
                Type = "text",
                Data = null
            };
        }

        #endregion

        #region Request & Response DTOs

        public class AiBotChatRequest
        {
            public string Message { get; set; } = string.Empty;
            public int? UserId { get; set; }
        }

        public class AiBotChatResponse
        {
            public string Reply { get; set; } = string.Empty;
            public string Type { get; set; } = "text";
            public object? Data { get; set; }
        }

        #endregion
    }
}

