using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using statgen.Models;

namespace statgen.Hubs
{
    public class StockStatsHub : Hub
    {
        public IEnumerable<StockViewDto> GetAllStocks()
        {
            var appleStock = new StockViewDto
            {
                Symbol = "APPL",
                Price = 158.44m
            };

            var msStock = new StockViewDto
            {
                Symbol = "MSFT",
                Price = 75.12m
            };

            var googleStock = new StockViewDto
            {
                Symbol = "GOOG",
                Price = 924.54m
            };

            appleStock.Price = 157.96m;
            msStock.Price = 75.54m;
            googleStock.Price = 925.37m;
            googleStock.Price = 923.42m;

            var myList = new List<StockViewDto>
            {
                appleStock,
                msStock,
                googleStock
            };

            return myList;
        }
    }
}