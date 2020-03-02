using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeServerAPI.Data;
using TradeServerAPI.Models;

namespace TradeServerAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(StockOrderContext context)
        {
            context.Database.EnsureCreated();

            // ORDER-DEPTH DATA
            // check if data in db already
            if (context.StockOrders.Any())
                return;

            var stockorders = new StockOrder[]
            {
                new StockOrder{ID=1,Name="Microsoft Corp",StockID=0,UserID=1,StockCount=100,PriceLimit=185.00,BuySell=0,Created=DateTime.Now},
                new StockOrder{ID=6,Name="Microsoft Corp",StockID=0,UserID=1,StockCount=100,PriceLimit=185.00,BuySell=0,Created=DateTime.Now.AddMinutes(-14.0)},
                new StockOrder{ID=2,Name="Microsoft Corp",StockID=0,UserID=2,StockCount=200,PriceLimit=184.00,BuySell=0,Created=DateTime.Now.AddMinutes(-20.0)},
                new StockOrder{ID=3,Name="Microsoft Corp",StockID=0,UserID=3,StockCount=50,PriceLimit=187.00,BuySell=1,Created=DateTime.Now.AddMinutes(-40.0)},
                new StockOrder{ID=4,Name="Microsoft Corp",StockID=0,UserID=4,StockCount=50,PriceLimit=187.00,BuySell=1,Created=DateTime.Now.AddMinutes(-41.0)},
                new StockOrder{ID=5,Name="Microsoft Corp",StockID=0,UserID=5,StockCount=125,PriceLimit=188.00,BuySell=1,Created=DateTime.Now.AddMinutes(-50.0)},
            };

            foreach (StockOrder s in stockorders)
            {
                context.StockOrders.Add(s);
            }

            //context.SaveChanges();


            // ORDER-TRADE-HISTORY DATA
            // check if data in db already
            if (context.StockTrades.Any())
                return;

            var stocktrades = new StockTrade[]
            {
                new StockTrade{ID=1,StockID=0,BuyerID=1,SellerID=3,StockCount=100,Price=185.00,Created=DateTime.Now.AddMinutes(-45.0)},
                new StockTrade{ID=2,StockID=0,BuyerID=3,SellerID=5,StockCount=120,Price=184.00,Created=DateTime.Now.AddMinutes(-57.0)},
                new StockTrade{ID=3,StockID=0,BuyerID=5,SellerID=1,StockCount=110,Price=186.00,Created=DateTime.Now.AddMinutes(-59.0)},
            };

            foreach (StockTrade s in stocktrades)
            {
                context.StockTrades.Add(s);
            }
            context.SaveChanges();


        }
        //public static void Initialize(StockTradeContext context)
        //{
        //    context.Database.EnsureCreated();

        //    // ORDER-TRADE-HISTORY DATA
        //    // check if data in db already
        //    if (context.StockTrades.Any())
        //        return;

        //    var stocktrades = new StockTrade[]
        //    {
        //        new StockTrade{ID=1,StockID=0,BuyerID=1,SellerID=3,StockCount=100,Price=185.00,Created=DateTime.Now.AddMinutes(-45.0)},
        //        new StockTrade{ID=2,StockID=0,BuyerID=3,SellerID=5,StockCount=120,Price=184.00,Created=DateTime.Now.AddMinutes(-57.0)},
        //        new StockTrade{ID=3,StockID=0,BuyerID=5,SellerID=1,StockCount=110,Price=186.00,Created=DateTime.Now.AddMinutes(-59.0)},
        //    };

        //    foreach (StockTrade s in stocktrades)
        //    {
        //        context.StockTrades.Add(s);
        //    }
        //    context.SaveChanges();
        //}
    }
}
