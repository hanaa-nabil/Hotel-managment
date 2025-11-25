using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.Common.Models.DTOs.PaymentDTOs
{
    public class BillingAddressDTO
    {
        public string? Line1 { get; set; }
        public string? Line2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; } 
    }
}
