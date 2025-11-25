using Hotel_Management.BLL.Interfaces;
using Hotel_Management.Common.Models.DTOs.PaymentDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Hotel_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            IBookingService bookingService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpPost("create-payment-intent")]
        [Authorize]
        public async Task<ActionResult<PaymentIntentResponseDTO>> CreatePaymentIntent(
            [FromBody] CreatePaymentIntentDTO dto)
        {
            try
            {
                var result = await _paymentService.CreatePaymentIntentAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent");
                return BadRequest(new { error = ex.Message });
            }
        }

        // Simple card payment - uses test payment method for testing
        [HttpPost("process-card-payment")]
        [Authorize]
        public async Task<ActionResult<PaymentConfirmationDTO>> ProcessCardPayment(string PaymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(PaymentIntentId);

                if (!paymentIntent.Metadata.ContainsKey("booking_id"))
                {
                    return BadRequest(new { error = "Booking ID not found in payment intent" });
                }

                var bookingId = int.Parse(paymentIntent.Metadata["booking_id"]);

                // Use Stripe test payment method (pm_card_visa)
                string paymentMethodId = "pm_card_visa";

                // Confirm payment with test payment method
                var confirmOptions = new PaymentIntentConfirmOptions
                {
                    PaymentMethod = paymentMethodId
                };

                paymentIntent = await service.ConfirmAsync(PaymentIntentId, confirmOptions);

                // Handle result
                if (paymentIntent.Status == "succeeded")
                {
                    await _bookingService.UpdatePaymentStatusAsync(bookingId, paymentIntent.Id);

                    return Ok(new PaymentConfirmationDTO
                    {
                        PaymentIntentId = paymentIntent.Id,
                        BookingId = bookingId,
                        Success = true,
                        Message = "Payment successful",
                        Status = paymentIntent.Status
                    });
                }

                return Ok(new PaymentConfirmationDTO
                {
                    PaymentIntentId = paymentIntent.Id,
                    BookingId = bookingId,
                    Success = false,
                    Message = $"Payment status: {paymentIntent.Status}",
                    Status = paymentIntent.Status
                });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing card payment");
                return BadRequest(new { error = ex.Message, code = ex.StripeError?.Code });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing card payment");
                return BadRequest(new { error = ex.Message });
            }
        }

       

        [HttpPost("check-payment-status/{paymentIntentId}")]
        [Authorize]
        public async Task<ActionResult<PaymentConfirmationDTO>> CheckPaymentStatus(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                if (!paymentIntent.Metadata.ContainsKey("booking_id"))
                {
                    return BadRequest(new { error = "Booking ID not found in payment intent" });
                }

                var bookingId = int.Parse(paymentIntent.Metadata["booking_id"]);

                if (paymentIntent.Status == "succeeded")
                {
                    await _bookingService.UpdatePaymentStatusAsync(bookingId, paymentIntent.Id);

                    return Ok(new PaymentConfirmationDTO
                    {
                        PaymentIntentId = paymentIntent.Id,
                        BookingId = bookingId,
                        Success = true,
                        Message = "Payment successful and booking confirmed",
                        Status = paymentIntent.Status
                    });
                }

                return Ok(new PaymentConfirmationDTO
                {
                    PaymentIntentId = paymentIntent.Id,
                    BookingId = bookingId,
                    Success = false,
                    Message = $"Payment status: {paymentIntent.Status}",
                    Status = paymentIntent.Status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment status");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("refund/{paymentIntentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RefundPayment(string paymentIntentId, [FromQuery] decimal? amount = null)
        {
            try
            {
                var result = await _paymentService.RefundPaymentAsync(paymentIntentId, amount);

                if (result)
                {
                    _logger.LogInformation($"Refund processed for payment intent {paymentIntentId}");
                }

                return Ok(new { success = result, message = result ? "Refund processed successfully" : "Refund failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("status/{paymentIntentId}")]
        [Authorize]
        public async Task<ActionResult> GetPaymentStatus(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                return Ok(new
                {
                    paymentIntentId = paymentIntent.Id,
                    status = paymentIntent.Status,
                    amount = paymentIntent.Amount / 100m,
                    currency = paymentIntent.Currency,
                    paymentMethod = paymentIntent.PaymentMethod,
                    bookingId = paymentIntent.Metadata.ContainsKey("booking_id")
                        ? int.Parse(paymentIntent.Metadata["booking_id"])
                        : (int?)null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}