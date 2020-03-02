using System.Collections.Generic;

namespace TradeFrontEnd.Models
{
    public class OrderResultViewModel
    {
       public IEnumerable<OrderViewModel> Histories { get; set; }
    }
}
