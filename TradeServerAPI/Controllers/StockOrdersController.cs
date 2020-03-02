using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeServerAPI.Models;
using TradeServerAPI.Data;

namespace TradeServerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockOrdersController : ControllerBase
    {
        private readonly StockOrderContext _context;

        //----------------------------------------
        // Utility functions
        //----------------------------------------

        private double getHighestBuy()
        {
            // get all buy-orders
            List<StockOrder> buyOrders = _context.StockOrders.ToListAsync().Result.FindAll(
                delegate (StockOrder stock)
                {
                    return stock.BuySell == 0;
                }
            );

            // sort the buy-orders by PriceLimit
            if (buyOrders.Any())
            {
                buyOrders.Sort(
                    delegate (StockOrder s1, StockOrder s2)
                    {
                        return s2.PriceLimit.CompareTo(s1.PriceLimit);
                    }
                );

                return buyOrders[0].PriceLimit;
            }

            return 0.0;
        }

        private double getLowestSell()
        {
            // get all sell-orders
            List<StockOrder> sellOrders = _context.StockOrders.ToListAsync().Result.FindAll(
                delegate (StockOrder stock)
                {
                    return stock.BuySell == 1;
                }
            );

            // sort the sell-orders by PriceLimit
            if (sellOrders.Any())
            {
                sellOrders.Sort(
                    delegate (StockOrder s1, StockOrder s2)
                    {
                        return s1.PriceLimit.CompareTo(s2.PriceLimit);
                    }
                );

                return sellOrders[0].PriceLimit;
            }

            return 0.0;
        }

        private int getTotalAmount(List<StockOrder> orders)
        {
            int counter = 0;

            foreach(StockOrder stock in orders)
            {
                counter = counter + stock.StockCount;
            }

            return counter;
        }

        private async Task<bool> updateStockOrder(StockOrder stock)
        {
            if (stock.StockCount == 0)
            {
                // remove order
                _context.StockOrders.Remove(stock);
            }
            else
            {
                // update
                _context.StockOrders.Update(stock);
            }

            await _context.SaveChangesAsync();

            return true;
        }

        // add order to history (buyer, seller)
        private async Task<bool> addOrderToHistory(StockOrder stockBuy, StockOrder stockSell)
        {
            StockTrade strade = new StockTrade();

            strade.BuyerID = stockBuy.UserID;
            strade.SellerID = stockSell.UserID;
            strade.StockID = stockBuy.StockID;
            strade.StockCount = stockBuy.StockCount;
            strade.Price = stockSell.PriceLimit;
            strade.Created = DateTime.Now;

            await _context.StockTrades.AddAsync(strade);

            await _context.SaveChangesAsync();

            return true;
        }


        // do the actual trade, number of stocks traded is returned
        private async Task<int> doTheTrade(StockOrder stockOrder)
        {
            if (stockOrder.BuySell == 0)
            {
                // buy-order to execute
                // get all sell-orders
                List<StockOrder> sellOrders = _context.StockOrders.ToListAsync().Result.FindAll(
                    delegate (StockOrder stock)
                    {
                        return stock.BuySell == 1;
                    }
                );

                // sort the sell-orders by PriceLimit
                if (sellOrders.Any())
                {
                    sellOrders.Sort(
                        delegate (StockOrder s1, StockOrder s2)
                        {
                            return s2.PriceLimit.CompareTo(s1.PriceLimit);
                        }
                    );

                    // get sell-orders affected/covered
                    List<StockOrder> orders = sellOrders.FindAll(
                        delegate (StockOrder stock)
                        {
                            return stock.PriceLimit <= stockOrder.PriceLimit;
                        }
                    );

                    // sort the found orders by creation time, the oldest first
                    orders.Sort(
                        delegate (StockOrder s1, StockOrder s2)
                        {
                            int comparePrice = s1.PriceLimit.CompareTo(s2.PriceLimit);
                            if (comparePrice == 0)
                            {
                                return s1.Created.CompareTo(s2.Created);
                            }
                            return comparePrice;
                        }
                    );

                    int totAmnt = getTotalAmount(orders);

                    StockOrder tmpOrder = new StockOrder();
                    tmpOrder = stockOrder;
                    int amntCompleted = 0, amntLeft = stockOrder.StockCount;

                    if (stockOrder.StockCount <= totAmnt)
                    {
                        // buy-order will go through completely
                        foreach (StockOrder sto in orders)
                        {
                            if (amntLeft == 0)
                            {
                                return tmpOrder.StockCount;
                            }

                            if (amntLeft < sto.StockCount)
                            {
                                // partial order will take place for current sell-order
                                sto.StockCount = sto.StockCount - amntLeft;
                                amntCompleted = amntCompleted + amntLeft;
                                tmpOrder.StockCount = amntLeft;
                                amntLeft = 0;

                                // the current sell-order needs to be updated
                                var res = await updateStockOrder(sto);

                                // add order to history (buyer, seller)
                                res = await addOrderToHistory(tmpOrder, sto);

                                return stockOrder.StockCount;
                            }
                            else
                            {
                                // current sell-order will be completed
                                amntCompleted = amntCompleted + sto.StockCount;
                                amntLeft = amntLeft - sto.StockCount;
                                tmpOrder.StockCount = sto.StockCount;
                                sto.StockCount = 0;

                                // the current sell-order needs to be updated/removed
                                var res = await updateStockOrder(sto);

                                // add order to history (buyer, seller)
                                res = await addOrderToHistory(tmpOrder, sto);
                            }
                        }
                    }
                    else
                    {
                        // buy-order will partly go through (stockOrder.StockCount > totAmnt)
                        foreach (StockOrder sto in orders)
                        {
                            if (amntLeft == 0)
                            {
                                return tmpOrder.StockCount;
                            }

                            if (amntLeft < sto.StockCount)
                            {
                                // partial order will take place for current sell-order
                                sto.StockCount = sto.StockCount - amntLeft;
                                amntCompleted = amntCompleted + amntLeft;
                                tmpOrder.StockCount = amntLeft;
                                amntLeft = 0;

                                // the current sell-order needs to be updated
                                var res = await updateStockOrder(sto);

                                // add order to history (buyer, seller)
                                res = await addOrderToHistory(tmpOrder, sto);

                                return stockOrder.StockCount;
                            }
                            else
                            {
                                // current sell-order will be completed
                                amntCompleted = amntCompleted + sto.StockCount; // 50, 100
                                amntLeft = amntLeft - sto.StockCount; // 950, 900
                                tmpOrder.StockCount = sto.StockCount; // 50, 50
                                sto.StockCount = 0;

                                // the current sell-order needs to be updated/removed
                                var res = await updateStockOrder(sto);

                                // add order to history (buyer, seller)
                                res = await addOrderToHistory(tmpOrder, sto);
                            }
                        }

                        // check if order depth needs to be updated with remaining buy amount
                        if ((totAmnt == amntCompleted) && (amntLeft > 0))
                        {
                            return amntCompleted;
                        }

                        return stockOrder.StockCount;
                    }

                    return amntCompleted;
                }
            }
            else
            {
                // sell-order to execute
                // get all buy-orders
                List<StockOrder> buyOrders = _context.StockOrders.ToListAsync().Result.FindAll(
                    delegate (StockOrder stock)
                    {
                        return stock.BuySell == 0;
                    }
                );

                // sort the buy-orders by PriceLimit
                if (buyOrders.Any())
                {
                    buyOrders.Sort(
                        delegate (StockOrder s1, StockOrder s2)
                        {
                            return s1.PriceLimit.CompareTo(s2.PriceLimit);
                        }
                    );

                    // get buy-orders affected/covered
                    List<StockOrder> orders = buyOrders.FindAll(
                        delegate (StockOrder stock)
                        {
                            return stock.PriceLimit >= stockOrder.PriceLimit;
                        }
                    );

                    // sort the found orders by creation time, the oldest first
                    orders.Sort(
                        delegate (StockOrder s1, StockOrder s2)
                        {
                            int comparePrice = s2.PriceLimit.CompareTo(s1.PriceLimit);
                            if (comparePrice == 0)
                            {
                                return s1.Created.CompareTo(s2.Created);
                            }
                            return comparePrice;
                        }
                    );

                    int totAmnt = getTotalAmount(orders);

                    StockOrder tmpOrder = new StockOrder();
                    tmpOrder = stockOrder;
                    int amntCompleted = 0, amntLeft = stockOrder.StockCount;

                    if (stockOrder.StockCount <= totAmnt)
                    {
                        // sell-order will go through completely
                        foreach (StockOrder sto in orders)
                        {
                            if (amntLeft == 0)
                            {
                                return tmpOrder.StockCount;
                            }

                            if (amntLeft < sto.StockCount)
                            {
                                // partial order will take place for current buy-order
                                sto.StockCount = sto.StockCount - amntLeft;
                                amntCompleted = amntCompleted + amntLeft;
                                tmpOrder.StockCount = amntLeft;
                                amntLeft = 0;

                                // the current buy-order needs to be updated
                                var res = await updateStockOrder(sto);

                                // add order to history (buyer, seller)
                                res = await addOrderToHistory(tmpOrder, sto);

                                return stockOrder.StockCount;
                            }
                            else
                            {
                                // current buy-order will be completed
                                amntCompleted = amntCompleted + sto.StockCount;
                                amntLeft = amntLeft - sto.StockCount;
                                tmpOrder.StockCount = sto.StockCount;
                                sto.StockCount = 0;

                                // the current buy-order needs to be updated/removed
                                var res = await updateStockOrder(sto);

                                // add order to history (buyer, seller)
                                res = await addOrderToHistory(tmpOrder, sto);
                            }
                        }
                    }
                    else
                    {
                        // sell-order will partly go through (stockOrder.StockCount > totAmnt)
                        foreach (StockOrder sto in orders)
                        {
                            if (amntLeft == 0)
                            {
                                return tmpOrder.StockCount;
                            }

                            if (amntLeft < sto.StockCount)
                            {
                                // partial order will take place for current buy-order
                                sto.StockCount = sto.StockCount - amntLeft;
                                amntCompleted = amntCompleted + amntLeft;
                                tmpOrder.StockCount = amntLeft;
                                amntLeft = 0;

                                // the current buy-order needs to be updated
                                var res = await updateStockOrder(sto);

                                // add order to history (buyer, seller)
                                res = await addOrderToHistory(tmpOrder, sto);

                                return stockOrder.StockCount;
                            }
                            else
                            {
                                // current buy-order will be completed
                                amntCompleted = amntCompleted + sto.StockCount;
                                amntLeft = amntLeft - sto.StockCount;
                                tmpOrder.StockCount = sto.StockCount;
                                sto.StockCount = 0;

                                // the current buy-order needs to be updated/removed
                                var res = await updateStockOrder(sto);

                                // add order to history (buyer, seller)
                                res = await addOrderToHistory(tmpOrder, sto);
                            }
                        }

                        // check if order depth needs to be updated with remaining buy amount
                        if ((totAmnt == amntCompleted) && (amntLeft > 0))
                        {
                            return amntCompleted;
                        }

                        return stockOrder.StockCount;
                    }

                    return amntCompleted;
                }
            }

                return 0;
        }

        //----------------------------------------
        // END Utility functions
        //----------------------------------------

        public StockOrdersController(StockOrderContext context)
        {
            _context = context;
        }

        // GET: api/StockOrders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockOrder>>> GetStockOrders()
        {
            // get all buy-orders
            List<StockOrder> buyOrders = _context.StockOrders.ToListAsync().Result.FindAll(
                delegate (StockOrder stock)
                {
                    return stock.BuySell == 0;
                }
            );

            // sort the buy-orders by PriceLimit
            if (buyOrders.Any())
            {
                buyOrders.Sort(
                    delegate (StockOrder s1, StockOrder s2)
                    {
                        return s2.PriceLimit.CompareTo(s1.PriceLimit);
                    }
                );
            }

            // get all sell-orders
            List<StockOrder> sellOrders = _context.StockOrders.ToListAsync().Result.FindAll(
                delegate (StockOrder stock)
                {
                    return stock.BuySell == 1;
                }
            );

            // sort the sell-orders by PriceLimit
            if (sellOrders.Any())
            {
                sellOrders.Sort(
                    delegate (StockOrder s1, StockOrder s2)
                    {
                        return s2.PriceLimit.CompareTo(s1.PriceLimit);
                    }
                );
            }

            return await _context.StockOrders.ToListAsync();
        }




        // GET: api/StockOrders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<StockTrade>>> GetStockOrder(long id)
        {
            return await _context.StockTrades.ToListAsync();
        }

        // PUT: api/StockOrders/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStockOrder(long id, StockOrder stockOrder)
        {
            if (id != stockOrder.ID)
            {
                return BadRequest();
            }

            _context.Entry(stockOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockOrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/StockOrders
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult> PostStockOrder(string stockOrderRequest)
        {
            var request = HttpContext.Request;

            IFormCollection reqStr = await request.ReadFormAsync();

            StockOrder stockOrder = new StockOrder();

            stockOrder.StockID = Convert.ToInt32(reqStr["StockID"]);
            stockOrder.Name = reqStr["Name"];
            stockOrder.Created = DateTime.Now;
            stockOrder.UserID = Convert.ToInt32(reqStr["UserID"]);
            stockOrder.StockCount = Convert.ToInt32(reqStr["StockCount"]);
            stockOrder.PriceLimit = Convert.ToDouble(reqStr["PriceLimit"]);
            stockOrder.BuySell = Convert.ToInt32(reqStr["BuySell"]);

            int tradedAmount = 0;

            if (stockOrder.BuySell == 0)
            {
                // This is a buy-order
                double lagst = getLowestSell();

                // if bid is lower than lowest sell-order, place it in orders
                if (stockOrder.PriceLimit < lagst)
                {
                    _context.StockOrders.Add(stockOrder);
                }
                else
                {
                    int origAmount = stockOrder.StockCount;
                    // do the buy-trade
                    tradedAmount = await doTheTrade(stockOrder);

                    // if not fully traded, update the order
                    if (tradedAmount == origAmount)
                    {
                        // fully traded
                    }
                    else
                    {
                        // add remaining order in order depth
                        stockOrder.StockCount = origAmount - tradedAmount;
                        _context.StockOrders.Add(stockOrder);
                    }
                }
            }
            else
            {
                // This is a sell-order
                double hogst = getHighestBuy();

                // if bid is higher than highest buy-order, place it in orders
                if (stockOrder.PriceLimit > hogst)
                {
                    _context.StockOrders.Add(stockOrder);
                }
                else
                {
                    int origAmount = stockOrder.StockCount;
                    // do the sell-trade
                    tradedAmount = await doTheTrade(stockOrder);

                    // if not fully traded, update the order
                    if (tradedAmount == origAmount)
                    {
                        // fully traded
                    }
                    else
                    {
                        // add remaining order in order depth
                        stockOrder.StockCount = origAmount - tradedAmount;
                        _context.StockOrders.Add(stockOrder);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(PostStockOrder), new { id = 1 }, stockOrderRequest);
        }


        // DELETE: api/StockOrders/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<StockOrder>> DeleteStockOrder(long id)
        {
            var stockOrder = await _context.StockOrders.FindAsync(id);
            if (stockOrder == null)
            {
                return NotFound();
            }

            _context.StockOrders.Remove(stockOrder);
            await _context.SaveChangesAsync();

            return stockOrder;
        }

        private bool StockOrderExists(long id)
        {
            return _context.StockOrders.Any(e => e.ID == id);
        }



    }
}
