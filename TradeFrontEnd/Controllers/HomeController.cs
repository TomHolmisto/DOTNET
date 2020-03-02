using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TradeFrontEnd.Models;
using System.Net.Http;
using System.Text.Json;
using Newtonsoft.Json;

namespace TradeFrontEnd.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new OrderFormViewModel();
            model.OrderType = 0; // 0:Buy, 1:Sell
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult>  Index(OrderFormViewModel orderForm)
        {
            using var client = new HttpClient();
            var queryString = new StringContent("asdfasdfasdf");
            var result = await client.PostAsync("http://webcode.me", queryString);

            var resultModel = new OrderFormViewModel();

            // get history of orders
            var res = await client.GetAsync("https://localhost:44346/api/stockorders");

            string str = await res.Content.ReadAsStringAsync();
            List<OrderViewModel> model = JsonConvert.DeserializeObject<List<OrderViewModel>>(str);

            var orderResultModel = new OrderResultViewModel
            {
                Histories = new List<OrderViewModel>(model)
            };

            resultModel.OrderResult = orderResultModel;

            return View(resultModel);
        }

        [HttpPost]
        public async Task<IActionResult> OrderFormView(OrderFormViewModel orderForm)
        {
            using var client = new HttpClient();
            var resultModel = new OrderFormViewModel();

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("StockID", orderForm.StockId.ToString()),
                new KeyValuePair<string, string>("Name", "Microsoft Corp"),
                new KeyValuePair<string, string>("UserID", orderForm.UserId.ToString()),
                new KeyValuePair<string, string>("StockCount", orderForm.Amount.ToString()),
                new KeyValuePair<string, string>("PriceLimit", orderForm.PriceLimit.ToString()),
                new KeyValuePair<string, string>("BuySell", orderForm.OrderType.ToString()),
            });
            
            var result = await client.PostAsync("https://localhost:44346/api/stockorders", formContent);

            // result after post
            var stringContent = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode)
            {
                resultModel.StockId = -1;
            }
            else
            {
                resultModel.StockId = 100;
            }

            return View(resultModel);
        }


        public IActionResult OrderFormView()
        {
            var resultModel = new OrderFormViewModel();

            return View(resultModel);
        }

        [HttpPost]
        public async Task<IActionResult> OrderDepthView(OrderDepthViewModel orderForm)
        {
            using var client = new HttpClient();

            var resultModel = new OrderDepthViewModel();

            var res = await client.GetAsync("https://localhost:44346/api/stockorders"); // gets current order depth

            string str = await res.Content.ReadAsStringAsync();
            List<OrderDepthViewModel> model = JsonConvert.DeserializeObject<List<OrderDepthViewModel>>(str);

            var historyResultModel = new OrderDepthViewResultModel
            {
                Histories = new List<OrderDepthViewModel>(model)
            };
            
            // separate the buy-orders from sell-orders
            List<OrderDepthViewModel> buyOrders = model.FindAll(
                delegate (OrderDepthViewModel stock)
                {
                    return stock.BuySell == 0;
                }
            );

            List<OrderDepthViewModel> sellOrders = model.FindAll(
                delegate (OrderDepthViewModel stock)
                {
                    return stock.BuySell == 1;
                }
            );

            // grouping buy
            var ordersGroupedByPriceBuy = buyOrders.GroupBy(od => od.PriceLimit);
            List<OrderDepthViewModel> buyOrdersOut = new List<OrderDepthViewModel>();
            foreach (var group in ordersGroupedByPriceBuy)
            {
                int counter = 0;
                OrderDepthViewModel odv = new OrderDepthViewModel();
                foreach (var ord in group)
                {
                    odv = ord;
                    counter = counter + ord.StockCount;
                }
                odv.StockCount = counter;
                buyOrdersOut.Add(odv);
            }

            // grouping sell
            var ordersGroupedByPrice = sellOrders.GroupBy(od => od.PriceLimit);
            List<OrderDepthViewModel> sellOrdersOut = new List<OrderDepthViewModel>();
            foreach(var group in ordersGroupedByPrice) 
            {
                int counter = 0;
                OrderDepthViewModel odv = new OrderDepthViewModel();
                foreach (var ord in group)
                {
                    odv = ord;
                    counter = counter + ord.StockCount;
                }
                odv.StockCount = counter;
                sellOrdersOut.Add(odv);
            }

            // sort the two lists by PriceLimit
            if (buyOrdersOut.Any())
            {
                buyOrdersOut.Sort(
                    delegate (OrderDepthViewModel s1, OrderDepthViewModel s2)
                    {
                        return s2.PriceLimit.CompareTo(s1.PriceLimit);
                    }
                );
            }
            // sort the sell-orders by PriceLimit
            if (sellOrdersOut.Any())
            {
                sellOrdersOut.Sort(
                    delegate (OrderDepthViewModel s1, OrderDepthViewModel s2)
                    {
                        return s1.PriceLimit.CompareTo(s2.PriceLimit);
                    }
                );
            }

            resultModel.OrderBuy = buyOrdersOut;
            resultModel.OrderSell = sellOrdersOut;
            resultModel.OrderDepthResult = historyResultModel;

            return View(resultModel);
        }


        public IActionResult OrderDepthView()
        {
            var resultModel = new OrderDepthViewModel();


            return View(resultModel);
        }

        [HttpPost]
        public async Task<IActionResult> HistoryView(HistoryViewModel historyForm)
        {
            using var client = new HttpClient();

            var resultModel = new HistoryViewModel();

            var res = await client.GetAsync("https://localhost:44346/api/stockorders/0"); // gets history of orders made
        

            string str = await res.Content.ReadAsStringAsync();
            List<HistoryViewModel> model = JsonConvert.DeserializeObject<List<HistoryViewModel>>(str);

            // sort
            if (model.Any())
            {
                model.Sort(
                    delegate (HistoryViewModel s1, HistoryViewModel s2)
                    {
                        return s2.Created.CompareTo(s1.Created);
                    }
                );
            }

            var historyResultModel = new HistoryViewResultModel
            {
                Histories = new List<HistoryViewModel>(model)
            };

            resultModel.HistoryResult = historyResultModel;

            return View(resultModel);
        }


        public IActionResult HistoryView()
        {
            var resultModel = new HistoryViewModel();

            return View(resultModel);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
