using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradeServerAPI.Data;
using TradeServerAPI.Models;

namespace TradeServerAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                //var services = scope.ServiceProvider;
                //var context = services.GetRequiredService<StockOrderContext>();
                //DbInitializer.Initialize(context);

                //var context2 = services.GetRequiredService<StockTradeContext>();
                //DbInitializer.Initialize(context2);


                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<StockOrderContext>();
                DbInitializer.Initialize(context);

                //var context2 = services.GetRequiredService<StockTradeContext>();
                //DbInitializer.Initialize(context, context2);

            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
