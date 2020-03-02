using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TradeServerAPI.Models
{
    public class StockTradeContext : DbContext
    {
    public StockTradeContext(DbContextOptions<StockTradeContext> options)
        : base(options)
    {
    }

    //public DbSet<StockTrade> StockTrades { get; set; }
}
}
