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

namespace TradeFrontEnd.Controllers
{
    public class HistoryController : Controller
    {
        private readonly ILogger<HistoryController> _logger;

        public HistoryController(ILogger<HistoryController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new HistoryViewModel();

            return View(model);
        }

        public IActionResult HistoryView()
        {
            var model = new HistoryViewModel();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Index(HistoryViewModel historyForm)
        {
            using var client = new HttpClient();

            var res = await client.GetAsync("https://localhost:44346/api/stockorders");

            var resultModel = new HistoryViewModel();

            //var historyResultModel = new 

            return View(resultModel);
        }
    }
}