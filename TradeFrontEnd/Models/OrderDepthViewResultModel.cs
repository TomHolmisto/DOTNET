using System;
using System.Collections.Generic;

namespace TradeFrontEnd.Models
{
    public class OrderDepthViewResultModel
    {
        public IEnumerable<OrderDepthViewModel> Histories { get; set; }
    }
}
