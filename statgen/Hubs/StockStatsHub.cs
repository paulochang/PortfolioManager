using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using statgen.Models;

namespace statgen.Hubs
{
    public class StockStatsHub : Hub
    {

        public IEnumerable<Stock> GetAllStocks()
        {
            Stock appleStock = new Stock
            {
                Symbol = "APPL",
                Price = 158.44m
            };

            Stock msStock = new Stock
            {
                Symbol = "MSFT",
                Price = 75.12m
            };

            Stock googleStock = new Stock
            {
                Symbol = "GOOG",
                Price = 924.54m
            };

            appleStock.Price = 157.96m;
            msStock.Price = 75.54m;
            googleStock.Price = 925.37m;
            googleStock.Price = 923.42m;

            List<Stock> myList = new List<Stock>{
                appleStock,
                msStock,
                googleStock
            };

            return myList;
        }
    }
}
