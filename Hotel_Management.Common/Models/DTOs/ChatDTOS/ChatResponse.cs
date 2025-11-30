using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.Common.Models.DTOs.ChatDTOS
{
    public class ChatResponse
    {
        public string Reply { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
    }
}
