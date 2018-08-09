using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using statgen.Models;

namespace statgen.Hubs
{
    public class StockStatsHub : Hub
    {

        public IEnumerable<StockViewDTO> GetAllStocks()
        {
            StockViewDTO appleStock = new StockViewDTO
            {
                Symbol = "APPL",
                Price = 158.44m
            };

            StockViewDTO msStock = new StockViewDTO
            {
                Symbol = "MSFT",
                Price = 75.12m
            };

            StockViewDTO googleStock = new StockViewDTO
            {
                Symbol = "GOOG",
                Price = 924.54m
            };

            appleStock.Price = 157.96m;
            msStock.Price = 75.54m;
            googleStock.Price = 925.37m;
            googleStock.Price = 923.42m;

            List<StockViewDTO> myList = new List<StockViewDTO>{
                appleStock,
                msStock,
                googleStock
            };

            return myList;
        }
    }
}
