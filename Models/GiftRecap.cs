using System;
using System.Collections.Generic;
using System.Text;

namespace BanggoodGiftExchange.Models
{
    public class GiftRecap
    {
        public string GiftDescription { get; set; }

        public string SenderName { get; set; }

        public string ReceiverName { get; set; }

        public DateTime? ReceivedDate { get; set; }
    }
}
