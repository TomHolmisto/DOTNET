using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TradeServerAPI.Models
{
    public class StockOrderContext : DbContext
    {
        public StockOrderContext(DbContextOptions<StockOrderContext> options)
            : base(options)
        {
        }

        public StockOrderContext() 
            : base()
        {
        }

        public DbSet<StockOrder> StockOrders { get; set; }
        public IEnumerable<StockOrder> Orders { get; set; }

        public DbSet<StockTrade> StockTrades { get; set; }
        public IEnumerable<StockTrade> Trades { get; set; }
    }
}
