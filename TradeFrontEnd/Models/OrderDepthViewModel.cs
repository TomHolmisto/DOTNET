using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeFrontEnd.Models
{
    public class OrderDepthViewModel
    {
        public long ID { get; set; }
        public int StockID { get; set; }
        public string Name { get; set; }
        public int UserID { get; set; }
        public int StockCount { get; set; }
        public double PriceLimit { get; set; }
        public int BuySell { get; set; }
        public DateTime Created { get; set; }

        public List<OrderDepthViewModel> OrderBuy { get; set; }
        public List<OrderDepthViewModel> OrderSell { get; set; }
        public OrderDepthViewResultModel OrderDepthResult { get; set; }
    }
}
