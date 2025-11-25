using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hotel_Management.Common.Models.DTOs.PaymentDTOs
{
    public class ProcessCardPaymentDTO
    {
        public string PaymentIntentId { get; set; }
    }

}
