using System;

namespace TelerikPerformanceDemo.Models
{
    public class OrderDetailViewModel
    {
        public int OrderDetailID { get; set; }
        public string AccountNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string ProductName { get; set; }
        public string OrderTrackingNumber { get; set; }
        public int OrderQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
