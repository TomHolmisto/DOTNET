using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeServerAPI.Models
{
    public class StockTrade
    {
        public long ID { get; set; }
        public int StockID { get; set; }
        public int BuyerID { get; set; }
        public int SellerID { get; set; }
        public int StockCount { get; set; }
        public double Price { get; set; }
        public DateTime Created { get; set; }
    }
}
