using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Hotel_Management.BLL.Interfaces;
using Hotel_Management.Common.Models.DTOs.HotelDTOS;
using Hotel_Management.Common.Models.DTOs.ChatDTOS;

namespace Hotel_Management.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomService _roomService;
        private readonly IUserService _userService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IBookingService bookingService,
            IRoomService roomService,
            IUserService userService,
            IPaymentService paymentService,
            ILogger<ChatController> logger)
        {
            _bookingService = bookingService;
            _roomService = roomService;
            _userService = userService;
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("message")]
        public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return BadRequest(new { error = "Message cannot be empty" });
            }

            try
            {
                _logger.LogInformation("Processing chatbot message: {Message}", request.Message);

                string reply = await HandleUserMessage(request.Message);

                return Ok(new ChatResponse
                {
                    Reply = reply,
                    Timestamp = DateTime.UtcNow,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chatbot message");
                return StatusCode(500, new ChatResponse
                {
                    Reply = "❌ حدث خطأ أثناء معالجة رسالتك. حاول مرة أخرى.",
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                status = "online",
                version = "1.0.0",
                supportedLanguages = new[] { "en", "ar" },
                timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("commands")]
        public IActionResult GetCommands()
        {
            var commands = new[]
            {
                new {
                    command = "show rooms / وريني الغرف المتاحة",
                    description = "Display all available rooms with prices",
                    example = "وريني الغرف المتاحة"
                },
                new {
                    command = "book room X / احجز غرفة رقم X",
                    description = "Book a specific room by room ID",
                    example = "احجز غرفة رقم 1"
                },
                new {
                    command = "my bookings / وريني حجوزاتي",
                    description = "Show all your bookings",
                    example = "وريني حجوزاتي"
                },
                new {
                    command = "booking number X / حجز رقم X",
                    description = "Get details of a specific booking",
                    example = "حجز رقم 5"
                },
                new {
                    command = "cancel booking X / الغى حجز رقم X",
                    description = "Cancel a specific booking",
                    example = "الغى حجز رقم 5"
                },
                new {
                    command = "my stats / احصائياتي",
                    description = "View your booking statistics",
                    example = "احصائياتي"
                },
                new {
                    command = "recent bookings / آخر حجوزاتي",
                    description = "View your recent bookings",
                    example = "آخر حجوزاتي"
                },
                new {
                    command = "spending summary / صرفت اد ايه",
                    description = "View your spending summary by month",
                    example = "صرفت اد ايه"
                }
            };

            return Ok(new { commands, totalCommands = commands.Length });
        }

        #region Private Helper Methods

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private async Task<string> HandleUserMessage(string msg)
        {
            msg = msg.ToLower().Trim();

            var roomKeywords = new List<string>
            {
                "room", "غرفة", "available rooms", "price", "type",
                "الغرف المتاحة", "الاسعار", "السعر", "show rooms"
            };

            // Check for bookings list
            if (msg.Contains("وريني حجوزاتي") || msg.Contains("حجوزاتي") ||
                msg.Contains("my bookings") || msg.Contains("show bookings"))
                return await GetMyBookings();

            // Check for specific booking by ID
            if (msg.Contains("حجز رقم") || msg.Contains("booking number") || msg.Contains("booking #"))
            {
                int bookingId = ExtractNumber(msg);
                return await GetBookingById(bookingId);
            }

            // Check for cancel booking
            if (msg.Contains("الغى حجز") || msg.Contains("cancel booking") || msg.Contains("الغاء"))
            {
                int bookingId = ExtractNumber(msg);
                return await CancelBooking(bookingId);
            }

            // Check for update booking
            if (msg.Contains("عدل حجز") || msg.Contains("change booking") || msg.Contains("update booking"))
            {
                int bookingId = ExtractNumber(msg);
                string newStatus = ExtractStatus(msg);
                return await UpdateBooking(bookingId, newStatus);
            }

            // Check for user statistics
            if (msg.Contains("احصائياتي") || msg.Contains("my stats") || msg.Contains("statistics"))
                return await GetUserStats();

            // Check for recent bookings
            if (msg.Contains("آخر حجوزاتي") || msg.Contains("recent bookings") || msg.Contains("latest"))
                return await GetRecentBookings();

            // Check for spending summary
            if (msg.Contains("صرفت اد ايه") || msg.Contains("صرفت كام") ||
                msg.Contains("spending summary") || msg.Contains("total spent"))
                return await GetSpendingSummary();

            // Check for create booking
            if (msg.Contains("احجز") || msg.Contains("book room") || msg.Contains("reserve"))
            {
                int roomId = ExtractNumber(msg);
                if (roomId > 0)
                    return await CreateBooking(roomId);
                else
                    return "❌ من فضلك حدد رقم الغرفة. مثال: احجز غرفة رقم 1";
            }

            // Check for room information
            if (roomKeywords.Exists(k => msg.Contains(k)))
                return await GetRooms();

            // Default response - show help
            return GetHelpMessage();
        }

        private int ExtractNumber(string msg)
        {
            var words = msg.Split(new[] { ' ', '،', ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                if (int.TryParse(word, out int number))
                    return number;

                var digits = new string(word.Where(char.IsDigit).ToArray());
                if (!string.IsNullOrEmpty(digits) && int.TryParse(digits, out int extractedNumber))
                    return extractedNumber;
            }

            return 0;
        }

        private string ExtractStatus(string msg)
        {
            if (msg.Contains("confirmed") || msg.Contains("مؤكد"))
                return "Confirmed";
            if (msg.Contains("pending") || msg.Contains("معلق"))
                return "Pending";
            if (msg.Contains("completed") || msg.Contains("مكتمل"))
                return "Completed";

            return "Pending";
        }

        private string GetHelpMessage()
        {
            return @"🤖 مرحباً! أنا مساعد الفندق الذكي

📋 الأوامر المتاحة:

🏨 **الغرف:**
   • وريني الغرف المتاحة - عرض جميع الغرف
   • احجز غرفة رقم X - حجز غرفة محددة

📖 **الحجوزات:**
   • وريني حجوزاتي - عرض كل حجوزاتك
   • حجز رقم X - تفاصيل حجز معين
   • الغى حجز رقم X - إلغاء حجز
   • آخر حجوزاتي - آخر 5 حجوزات

📊 **الإحصائيات:**
   • احصائياتي - عرض احصائياتك الكاملة
   • صرفت اد ايه - ملخص المصروفات

❓ اكتب أي أمر من الأوامر أعلاه للبدء!";
        }

        #endregion

        #region Service Calls

        private async Task<string> GetRooms()
        {
            try
            {
                var rooms = await _roomService.GetAllRoomsAsync();

                if (rooms == null || !rooms.Any())
                {
                    return "📭 لا توجد غرف متاحة حالياً";
                }

                var replyLines = new List<string> { "🏨 **الغرف المتاحة:**\n" };

                foreach (var room in rooms)
                {
                    string status = room.IsAvailable ? "✅ متاحة" : "❌ محجوزة";

                    replyLines.Add(
                        $"🔹 غرفة #{room.Id} - {room.RoomNumber}\n" +
                        $"   النوع: {room.Type}\n" +
                        $"   السعر: ${room.PricePerNight}/ليلة\n" +
                        $"   الحالة: {status}\n"
                    );
                }

                replyLines.Add("\n💡 لحجز غرفة، اكتب: احجز غرفة رقم X (استخدم رقم الغرفة ID)");

                return string.Join("\n", replyLines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching rooms");
                return "❌ معرفتش أجيبلك بيانات الغرف. حاول تاني بعد شوية.";
            }
        }

        private async Task<string> CreateBooking(int roomId)
        {
            try
            {
                var userId = GetUserId();

                var room = await _roomService.GetRoomByIdAsync(roomId);
                if (room == null)
                {
                    return $"❌ الغرفة رقم {roomId} غير موجودة. تأكد من رقم الغرفة.";
                }

                if (!room.IsAvailable)
                {
                    return $"❌ الغرفة رقم {roomId} محجوزة حالياً. جرب غرفة أخرى.";
                }

                var createBookingDto = new CreateBookingDTO
                {
                    RoomId = roomId,
                    CheckIn = DateTime.UtcNow.Date,
                    CheckOut = DateTime.UtcNow.Date.AddDays(1)
                };

                var booking = await _bookingService.CreateBookingAsync(userId, createBookingDto);

                return $"✅ **تم الحجز بنجاح!**\n\n" +
                       $"🔖 رقم الحجز: {booking.Id}\n" +
                       $"🏨 الغرفة: {booking.RoomNumber}\n" +
                       $"🏢 الفندق: {booking.HotelName}\n" +
                       $"📅 تسجيل الدخول: {booking.CheckIn:yyyy-MM-dd}\n" +
                       $"📅 تسجيل الخروج: {booking.CheckOut:yyyy-MM-dd}\n" +
                       $"💰 المبلغ الإجمالي: ${booking.TotalPrice}\n" +
                       $"📊 الحالة: {booking.Status}\n\n" +
                       $"✨ نتمنى لك إقامة سعيدة!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking for room {RoomId}", roomId);
                return $"❌ معرفتش أعمل الحجز. السبب: {ex.Message}";
            }
        }

        private async Task<string> GetMyBookings()
        {
            try
            {
                var userId = GetUserId();
                var bookings = await _bookingService.GetUserBookingsAsync(userId);

                if (bookings == null || !bookings.Any())
                {
                    return "📭 ما عندكش أي حجوزات حالياً\n\n💡 لحجز غرفة، اكتب: احجز غرفة رقم X";
                }

                var lines = new List<string> { $"📋 **حجوزاتك ({bookings.Count()}):**\n" };

                foreach (var b in bookings)
                {
                    var statusEmoji = b.Status switch
                    {
                        "Confirmed" => "✅",
                        "Pending" => "⏳",
                        "Cancelled" => "❌",
                        "Completed" => "✔️",
                        _ => "📌"
                    };

                    lines.Add(
                        $"{statusEmoji} **الحجز #{b.Id}**\n" +
                        $"🏨 الغرفة: {b.RoomNumber}\n" +
                        $"🏢 الفندق: {b.HotelName}\n" +
                        $"📅 {b.CheckIn:yyyy-MM-dd} → {b.CheckOut:yyyy-MM-dd}\n" +
                        $"💰 ${b.TotalPrice}\n" +
                        $"📊 الحالة: {b.Status}\n"
                    );
                }

                return string.Join("\n", lines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user bookings");
                return "❌ معرفتش أجيب حجوزاتك. حاول تاني.";
            }
        }

        private async Task<string> GetBookingById(int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);

                if (booking == null)
                {
                    return $"❌ معرفتش ألاقي الحجز رقم {id}\n\n💡 تأكد من رقم الحجز وحاول تاني";
                }

                var userId = GetUserId();
                if (booking.UserId != userId && !User.IsInRole("Admin"))
                {
                    return "❌ هذا الحجز لا يخصك!";
                }

                var statusEmoji = booking.Status switch
                {
                    "Confirmed" => "✅",
                    "Pending" => "⏳",
                    "Cancelled" => "❌",
                    "Completed" => "✔️",
                    _ => "📌"
                };

                return $"{statusEmoji} **تفاصيل الحجز #{booking.Id}**\n\n" +
                       $"🏨 الغرفة: {booking.RoomNumber}\n" +
                       $"🏢 الفندق: {booking.HotelName}\n" +
                       $"📅 تسجيل الدخول: {booking.CheckIn:yyyy-MM-dd}\n" +
                       $"📅 تسجيل الخروج: {booking.CheckOut:yyyy-MM-dd}\n" +
                       $"💰 المبلغ: ${booking.TotalPrice}\n" +
                       $"📊 الحالة: {booking.Status}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching booking {BookingId}", id);
                return "❌ حدث خطأ أثناء جلب بيانات الحجز";
            }
        }

        private async Task<string> CancelBooking(int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return $"❌ الحجز رقم {id} غير موجود";
                }

                var userId = GetUserId();
                if (booking.UserId != userId && !User.IsInRole("Admin"))
                {
                    return "❌ لا يمكنك إلغاء حجز لا يخصك!";
                }

                var result = await _bookingService.CancelBookingAsync(id);

                if (result)
                {
                    return $"✅ تم إلغاء الحجز رقم {id} بنجاح\n\n💡 يمكنك عمل حجز جديد في أي وقت!";
                }
                else
                {
                    return $"❌ معرفتش ألغي الحجز رقم {id}. الحجز قد يكون ملغي مسبقاً.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling booking {BookingId}", id);
                return "❌ حدث خطأ أثناء إلغاء الحجز";
            }
        }

        private async Task<string> UpdateBooking(int id, string newStatus)
        {
            try
            {
                var booking = await _bookingService.UpdateBookingStatusAsync(id, newStatus);

                if (booking == null)
                {
                    return $"❌ معرفتش أعدل الحجز رقم {id}";
                }

                return $"✅ **تم تعديل الحجز #{id}**\n\n" +
                       $"🏨 الغرفة: {booking.RoomNumber}\n" +
                       $"🏢 الفندق: {booking.HotelName}\n" +
                       $"📊 الحالة الجديدة: {booking.Status}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking {BookingId}", id);
                return "❌ حدث خطأ أثناء تعديل الحجز";
            }
        }

        private async Task<string> GetUserStats()
        {
            try
            {
                var userId = GetUserId();
                var bookings = await _bookingService.GetUserBookingsAsync(userId);

                if (bookings == null || !bookings.Any())
                {
                    return "📊 لا توجد إحصائيات بعد. ابدأ بعمل أول حجز!";
                }

                var stats = new
                {
                    TotalBookings = bookings.Count(),
                    ActiveBookings = bookings.Count(b => b.Status == "Confirmed"),
                    PastBookings = bookings.Count(b => b.Status == "Completed"),
                    CancelledBookings = bookings.Count(b => b.Status == "Cancelled"),
                    PendingBookings = bookings.Count(b => b.Status == "Pending"),
                    TotalSpent = bookings.Where(b => b.Status != "Cancelled").Sum(b => b.TotalPrice),
                    UpcomingCheckIns = bookings.Count(b =>
                        b.Status == "Confirmed" &&
                        b.CheckIn > DateTime.UtcNow &&
                        b.CheckIn <= DateTime.UtcNow.AddDays(7))
                };

                return $"📊 **إحصائياتك:**\n\n" +
                       $"📋 إجمالي الحجوزات: {stats.TotalBookings}\n" +
                       $"✅ الحجوزات الفعالة: {stats.ActiveBookings}\n" +
                       $"📅 الحجوزات السابقة: {stats.PastBookings}\n" +
                       $"❌ الحجوزات الملغية: {stats.CancelledBookings}\n" +
                       $"⏳ المعلقة: {stats.PendingBookings}\n" +
                       $"💰 المصروف الكلي: ${stats.TotalSpent:F2}\n" +
                       $"🔜 قادم للتحقق (خلال 7 أيام): {stats.UpcomingCheckIns}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user stats");
                return "❌ معرفتش أجيب الإحصائيات. حاول تاني.";
            }
        }

        private async Task<string> GetRecentBookings()
        {
            try
            {
                var userId = GetUserId();
                var bookings = await _bookingService.GetUserBookingsAsync(userId);

                if (bookings == null || !bookings.Any())
                {
                    return "📭 ما فيش حجوزات حديثة";
                }

                var recentBookings = bookings
                    .OrderByDescending(b => b.CheckIn)
                    .Take(5)
                    .ToList();

                var lines = new List<string> { "📋 **آخر 5 حجوزات:**\n" };

                foreach (var b in recentBookings)
                {
                    var statusEmoji = b.Status switch
                    {
                        "Confirmed" => "✅",
                        "Pending" => "⏳",
                        "Cancelled" => "❌",
                        "Completed" => "✔️",
                        _ => "📌"
                    };

                    lines.Add(
                        $"{statusEmoji} **#{b.Id}** | غرفة {b.RoomNumber}\n" +
                        $"📅 {b.CheckIn:yyyy-MM-dd} → {b.CheckOut:yyyy-MM-dd}\n" +
                        $"💰 ${b.TotalPrice} | {b.Status}\n"
                    );
                }

                return string.Join("\n", lines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent bookings");
                return "❌ معرفتش أجيب آخر حجوزاتك. حاول تاني.";
            }
        }

        private async Task<string> GetSpendingSummary()
        {
            try
            {
                var userId = GetUserId();
                var bookings = await _bookingService.GetUserBookingsAsync(userId);

                if (bookings == null || !bookings.Any())
                {
                    return "📭 ما فيش مصروفات حتى الآن";
                }

                var currentYear = DateTime.UtcNow.Year;
                var monthlySpending = bookings
                    .Where(b => b.CheckIn.Year == currentYear && b.Status != "Cancelled")
                    .GroupBy(b => b.CheckIn.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        MonthName = new DateTime(currentYear, g.Key, 1).ToString("MMMM"),
                        TotalSpent = g.Sum(b => b.TotalPrice),
                        BookingCount = g.Count()
                    })
                    .OrderBy(x => x.Month)
                    .ToList();

                if (!monthlySpending.Any())
                {
                    return $"📭 ما فيش مصروفات في سنة {currentYear}";
                }

                var lines = new List<string> { $"💰 **ملخص المصروفات ({currentYear}):**\n" };
                decimal total = 0;

                foreach (var s in monthlySpending)
                {
                    total += s.TotalSpent;

                    lines.Add(
                        $"📅 {s.MonthName}\n" +
                        $"   💵 ${s.TotalSpent:F2}\n" +
                        $"   📊 {s.BookingCount} حجز\n"
                    );
                }

                lines.Add($"\n💰 **الإجمالي: ${total:F2}**");

                return string.Join("\n", lines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching spending summary");
                return "❌ معرفتش أجيب ملخص المصروفات. حاول تاني.";
            }
        }

        #endregion
    }
}