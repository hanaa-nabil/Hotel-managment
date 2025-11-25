using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.Common.Models.DTOs.PaymentDTOs
{
    public class ConfirmWithPaymentMethodDTO
    {
        public string PaymentIntentId { get; set; }
        public string PaymentMethodId { get; set; }
    }
}
