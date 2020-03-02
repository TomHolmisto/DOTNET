using System;

namespace TradeFrontEnd.Models
{
    public class HistoryViewModel
    {

        public long Id { get; set; }
        public int StockID { get; set; }
        public string Name { get; set; }
        public int BuyerID { get; set; }
        public int SellerID { get; set; }
        public int StockCount { get; set; }
        public double Price { get; set; }
        public DateTime Created { get; set; }

        public HistoryViewResultModel HistoryResult { get; set; }
    }
}
