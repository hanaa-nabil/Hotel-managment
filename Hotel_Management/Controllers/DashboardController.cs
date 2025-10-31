using Hotel_Management.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hotel_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomService _roomService;
        private readonly IUserService _userService;

        public DashboardController(
            IBookingService bookingService,
            IRoomService roomService,
            IUserService userService)
        {
            _bookingService = bookingService;
            _roomService = roomService;
            _userService = userService;
        }

        // ==================== USER DASHBOARD ====================

        [HttpGet("user/stats")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUserStats()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bookings = await _bookingService.GetUserBookingsAsync(userId);

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

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user statistics", error = ex.Message });
            }
        }

        [HttpGet("user/recent-bookings")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUserRecentBookings([FromQuery] int count = 5)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bookings = await _bookingService.GetUserBookingsAsync(userId);

                var recentBookings = bookings
                    .OrderByDescending(b => b.CheckIn)
                    .Take(count)
                    .Select(b => new
                    {
                        b.Id,
                        b.RoomId,
                        b.CheckIn,
                        b.CheckOut,
                        b.TotalPrice,
                        b.Status,
                        DaysUntilCheckIn = (b.CheckIn.Date - DateTime.UtcNow.Date).Days,
                        Duration = (b.CheckOut.Date - b.CheckIn.Date).Days
                    })
                    .ToList();

                return Ok(recentBookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving recent bookings", error = ex.Message });
            }
        }

        [HttpGet("user/upcoming-bookings")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUserUpcomingBookings()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bookings = await _bookingService.GetUserBookingsAsync(userId);

                var upcomingBookings = bookings
                    .Where(b => b.Status == "Confirmed" && b.CheckIn >= DateTime.UtcNow)
                    .OrderBy(b => b.CheckIn)
                    .Select(b => new
                    {
                        b.Id,
                        b.RoomId,
                        b.CheckIn,
                        b.CheckOut,
                        b.TotalPrice,
                        b.Status,
                        DaysUntilCheckIn = (b.CheckIn.Date - DateTime.UtcNow.Date).Days
                    })
                    .ToList();

                return Ok(upcomingBookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving upcoming bookings", error = ex.Message });
            }
        }

        [HttpGet("user/spending-summary")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUserSpendingSummary()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bookings = await _bookingService.GetUserBookingsAsync(userId);

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

                return Ok(monthlySpending);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving spending summary", error = ex.Message });
            }
        }

        // ==================== ADMIN DASHBOARD ====================

        [HttpGet("admin/stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminStats()
        {
            try
            {
                var allBookings = await _bookingService.GetAllBookingsAsync();
                var allRooms = await _roomService.GetAllRoomsAsync();
                var allUsers = await _userService.GetAllUsersAsync();

                var today = DateTime.UtcNow.Date;
                var thisMonth = new DateTime(today.Year, today.Month, 1);
                var lastMonth = thisMonth.AddMonths(-1);

                var thisMonthRevenue = allBookings
                    .Where(b => b.CheckIn >= thisMonth && b.Status != "Cancelled")
                    .Sum(b => b.TotalPrice);

                var lastMonthRevenue = allBookings
                    .Where(b => b.CheckIn >= lastMonth && b.CheckIn < thisMonth && b.Status != "Cancelled")
                    .Sum(b => b.TotalPrice);

                var revenueGrowth = lastMonthRevenue > 0
                    ? ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100
                    : 0;

                var stats = new
                {
                    TotalBookings = allBookings.Count(),
                    TotalRevenue = allBookings.Where(b => b.Status != "Cancelled").Sum(b => b.TotalPrice),
                    MonthlyRevenue = thisMonthRevenue,
                    RevenueGrowth = Math.Round(revenueGrowth, 2),
                    TotalRooms = allRooms.Count(),
                    AvailableRooms = allRooms.Count(r => r.IsAvailable),
                    OccupiedRooms = allRooms.Count(r => !r.IsAvailable),
                    TotalUsers = allUsers.Count(),
                    ActiveBookings = allBookings.Count(b => b.Status == "Confirmed"),
                    PendingBookings = allBookings.Count(b => b.Status == "Pending"),
                    TodayCheckIns = allBookings.Count(b => b.CheckIn.Date == today && b.Status == "Confirmed"),
                    TodayCheckOuts = allBookings.Count(b => b.CheckOut.Date == today && b.Status == "Confirmed"),
                    OccupancyRate = allRooms.Count() > 0
                        ? Math.Round((double)(allRooms.Count() - allRooms.Count(r => r.IsAvailable)) / allRooms.Count() * 100, 2)
                        : 0
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving admin statistics", error = ex.Message });
            }
        }

        [HttpGet("admin/recent-bookings")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminRecentBookings([FromQuery] int count = 10)
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();

                var recentBookings = bookings
                    .OrderByDescending(b => b.Id)
                    .Take(count)
                    .Select(b => new
                    {
                        b.Id,
                        b.UserId,
                        b.RoomId,
                        b.CheckIn,
                        b.CheckOut,
                        b.TotalPrice,
                        b.Status,
                        Duration = (b.CheckOut.Date - b.CheckIn.Date).Days
                    })
                    .ToList();

                return Ok(recentBookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving recent bookings", error = ex.Message });
            }
        }

        [HttpGet("admin/revenue-by-month")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRevenueByMonth([FromQuery] int year)
        {
            try
            {
                if (year == 0) year = DateTime.UtcNow.Year;

                var bookings = await _bookingService.GetAllBookingsAsync();

                var revenueByMonth = bookings
                    .Where(b => b.CheckIn.Year == year && b.Status != "Cancelled")
                    .GroupBy(b => b.CheckIn.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        MonthName = new DateTime(year, g.Key, 1).ToString("MMMM"),
                        Revenue = g.Sum(b => b.TotalPrice),
                        BookingCount = g.Count(),
                        AverageBookingValue = g.Average(b => b.TotalPrice)
                    })
                    .OrderBy(x => x.Month)
                    .ToList();

                return Ok(revenueByMonth);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving revenue by month", error = ex.Message });
            }
        }

        [HttpGet("admin/room-type-stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoomTypeStats()
        {
            try
            {
                var rooms = await _roomService.GetAllRoomsAsync();
                var bookings = await _bookingService.GetAllBookingsAsync();

                var roomTypeStats = rooms
                    .GroupBy(r => r.Type)
                    .Select(g => new
                    {
                        RoomType = g.Key,
                        TotalRooms = g.Count(),
                        AvailableRooms = g.Count(r => r.IsAvailable),
                        OccupiedRooms = g.Count(r => !r.IsAvailable),
                        BookingCount = bookings.Count(b => g.Any(r => r.Id == b.RoomId)),
                        Revenue = bookings
                            .Where(b => g.Any(r => r.Id == b.RoomId) && b.Status != "Cancelled")
                            .Sum(b => b.TotalPrice),
                        AveragePricePerNight = g.Average(r => r.PricePerNight),
                        OccupancyRate = g.Count() > 0
                            ? Math.Round((double)g.Count(r => !r.IsAvailable) / g.Count() * 100, 2)
                            : 0
                    })
                    .OrderByDescending(x => x.Revenue)
                    .ToList();

                return Ok(roomTypeStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving room type statistics", error = ex.Message });
            }
        }

        [HttpGet("admin/booking-status-summary")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBookingStatusSummary()
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();

                var statusSummary = bookings
                    .GroupBy(b => b.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        TotalRevenue = g.Sum(b => b.TotalPrice),
                        Percentage = Math.Round((double)g.Count() / bookings.Count() * 100, 2)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                return Ok(statusSummary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving booking status summary", error = ex.Message });
            }
        }

        [HttpGet("admin/top-customers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTopCustomers([FromQuery] int count = 10)
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();
                var users = await _userService.GetAllUsersAsync();

                var topCustomers = bookings
                    .Where(b => b.Status != "Cancelled")
                    .GroupBy(b => b.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        User = users.FirstOrDefault(u => u.Id == g.Key),
                        TotalBookings = g.Count(),
                        TotalSpent = g.Sum(b => b.TotalPrice),
                        AverageBookingValue = g.Average(b => b.TotalPrice),
                        LastBookingDate = g.Max(b => b.CheckIn)
                    })
                    .OrderByDescending(x => x.TotalSpent)
                    .Take(count)
                    .ToList();

                return Ok(topCustomers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving top customers", error = ex.Message });
            }
        }
    }
}
