using Hotel_Management.Common.Models.DTOs.PaymentDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.BLL.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentIntentResponseDTO> CreatePaymentIntentAsync(CreatePaymentIntentDTO dto);
        Task<PaymentConfirmationDTO> ConfirmPaymentAsync(string paymentIntentId);
        Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null);
        //Task HandleWebhookAsync(string json, string signature);
    }
}
