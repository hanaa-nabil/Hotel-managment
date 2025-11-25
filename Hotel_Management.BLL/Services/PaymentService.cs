using Hotel_Management.BLL.Interfaces;
using Hotel_Management.Common.Models.DTOs.PaymentDTOs;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Hotel_Management.BLL.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;

        public PaymentService(IConfiguration configuration)
        {
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<PaymentIntentResponseDTO> CreatePaymentIntentAsync(CreatePaymentIntentDTO dto)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(dto.Amount * 100), 
                Currency = dto.Currency ?? "usd",
                PaymentMethodTypes = new List<string> { "card" }, 
                Description = dto.Description ?? $"Booking #{dto.BookingId}",
                Metadata = new Dictionary<string, string>
                {
                    { "booking_id", dto.BookingId.ToString() }
                },
                CaptureMethod = "automatic"
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return new PaymentIntentResponseDTO
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Amount = paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency,
                Status = paymentIntent.Status
            };
        }

        public async Task<PaymentConfirmationDTO> ConfirmPaymentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            return new PaymentConfirmationDTO
            {
                PaymentIntentId = paymentIntent.Id,
                BookingId = paymentIntent.Metadata.ContainsKey("booking_id")
                    ? int.Parse(paymentIntent.Metadata["booking_id"])
                    : 0,
                Success = paymentIntent.Status == "succeeded",
                Message = paymentIntent.Status == "succeeded"
                    ? "Payment successful"
                    : $"Payment status: {paymentIntent.Status}",
                Status = paymentIntent.Status
            };
        }

        public async Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null)
        {
            try
            {
                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.GetAsync(paymentIntentId);

                if (paymentIntent.Status != "succeeded")
                {
                    return false;
                }

                var refundService = new RefundService();
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId
                };

                if (amount.HasValue)
                {
                    refundOptions.Amount = (long)(amount.Value * 100);
                }

                var refund = await refundService.CreateAsync(refundOptions);
                return refund.Status == "succeeded" || refund.Status == "pending";
            }
            catch (StripeException)
            {
                return false;
            }
        }

    }
}