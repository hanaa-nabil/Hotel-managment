using Hotel_Management.BLL.Interfaces;
using Hotel_Management.Common.Models.DTOs.RolesDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IBookingService _bookingService;

        public AdminController(IUserService userService, IBookingService bookingService)
        {
            _userService = userService;
            _bookingService = bookingService;
        }

        // User Management
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("users/{userId}/assign-role")]
        public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleDTO model)
        {
            var result = await _userService.AssignRoleAsync(userId, model.RoleName);
            if (!result)
                return BadRequest("Failed to assign role");

            return Ok("Role assigned successfully");
        }

        [HttpPost("users/{userId}/remove-role")]
        public async Task<IActionResult> RemoveRole(string userId, [FromBody] AssignRoleDTO model)
        {
            var result = await _userService.RemoveRoleAsync(userId, model.RoleName);
            if (!result)
                return BadRequest("Failed to remove role");

            return Ok("Role removed successfully");
        }

        [HttpGet("users/{userId}/roles")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var roles = await _userService.GetUserRolesAsync(userId);
            return Ok(roles);
        }

        // Booking Management
        [HttpGet("bookings")]
        public async Task<IActionResult> GetAllBookings([FromQuery] string status = null)
        {
            var bookings = await _bookingService.GetAllBookingsAsync();

            if (!string.IsNullOrEmpty(status))
            {
                bookings = bookings.Where(b => b.Status == status).ToList();
            }

            return Ok(bookings);
        }

        [HttpPut("bookings/{id}/approve")]
        public async Task<IActionResult> ApproveBooking(int id)
        {
            var booking = await _bookingService.UpdateBookingStatusAsync(id, "Confirmed");
            if (booking == null)
                return NotFound();

            return Ok(booking);
        }

        [HttpPut("bookings/{id}/reject")]
        public async Task<IActionResult> RejectBooking(int id)
        {
            var booking = await _bookingService.UpdateBookingStatusAsync(id, "Cancelled");
            if (booking == null)
                return NotFound();

            return Ok(booking);
        }

        [HttpPut("bookings/{id}/complete")]
        public async Task<IActionResult> CompleteBooking(int id)
        {
            var booking = await _bookingService.UpdateBookingStatusAsync(id, "Completed");
            if (booking == null)
                return NotFound();

            return Ok(booking);
        }
    }
}
