using Hotel_Management.BLL.Interfaces;
using Hotel_Management.Common.Models.DTOs.HotelDTOS;
using Hotel_Management.Common.Models.DTOs.PaymentDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hotel_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IPaymentService _paymentService;

        public BookingsController(
            IBookingService bookingService,
            IPaymentService paymentService)
        {
            _bookingService = bookingService;
            _paymentService = paymentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            return Ok(bookings);
        }

        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
                return NotFound();
            return Ok(booking);
        }

        // NEW: Create booking with payment intent
        [HttpPost("create-with-payment")]
        public async Task<IActionResult> CreateWithPayment([FromBody] CreateBookingDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Create booking with payment setup
                var bookingWithPayment = await _bookingService.CreateBookingWithPaymentAsync(model, userId);

                // Create payment intent
                var paymentIntentDto = new CreatePaymentIntentDTO
                {
                    BookingId = bookingWithPayment.BookingId,
                    Amount = bookingWithPayment.TotalAmount,
                    Currency = "usd",
                    Description = $"Hotel Booking #{bookingWithPayment.BookingId}"
                };

                var paymentIntent = await _paymentService.CreatePaymentIntentAsync(paymentIntentDto);

                // Return combined response
                return Ok(new
                {
                    bookingId = bookingWithPayment.BookingId,
                    totalAmount = bookingWithPayment.TotalAmount,
                    paymentIntentId = paymentIntent.PaymentIntentId,
                    clientSecret = paymentIntent.ClientSecret,
                    status = paymentIntent.Status
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // OLD: Keep for backward compatibility (but it won't handle payment properly)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var booking = await _bookingService.CreateBookingAsync(userId, model);
                return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{bookingid}/cancel")]
        public async Task<IActionResult> Cancel(int bookingid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _bookingService.CancelBookingAsync(bookingid);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var booking = await _bookingService.UpdateBookingStatusAsync(id, status);
            if (booking == null)
                return NotFound();

            return Ok(booking);
        }
    }
}