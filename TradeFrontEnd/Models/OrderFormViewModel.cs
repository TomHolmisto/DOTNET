namespace TradeFrontEnd.Models
{
    public class OrderFormViewModel
    {
        public int OrderType { get; set; }
        public int StockId { get; set; }
        public int UserId { get; set; }
        public int Amount { get; set; }
        public double PriceLimit { get; set; }

        public OrderResultViewModel OrderResult { get; set; }
    }
}
