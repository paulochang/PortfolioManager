using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using statgen.Hubs;
using statgen.Models;
using statgen.SQLite;
using System.Linq;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace statgen.Controllers
{

    [ApiController]
    public class PortfolioController : ControllerBase
    {
        readonly IHubContext<StockStatsHub> hubContext;
        private readonly PortfolioContext portfolioContext;

        public PortfolioController(IHubContext<StockStatsHub> hubContext, PortfolioContext portfolioContext)
        {
            this.hubContext = hubContext;
            this.portfolioContext = portfolioContext;
        }

        [HttpPut("/price")]
        public Task PostPriceChange(StockPriceChange stockPriceChange)
        {
            return this.hubContext.Clients.All.SendAsync("Update", stockPriceChange);
        }

        [HttpGet("/portfolio/{id}")]
        public async Task<ActionResult<IEnumerable<PortfolioComponentViewDTO>>> Get(int id)
        {
            return await this.portfolioContext.PortfolioAllocations
                                                         .Where(p => p.PortfolioId == id)
                                                         .Select(p =>
                                                                 new PortfolioComponentViewDTO(
                                                                     p.StockId,
                                                                     p.Qty,
                                                                     p.Stock.Name,
                                                                     p.Stock.LatestPrice,
                                                                     (p.Stock.LatestPrice * p.Qty)
                                                                    )
                                                            ).ToListAsync();
        }

        [HttpPut("/stock/{id}/price")]
        public async Task PutStockPriceChange(int id, [FromBody] double stockPriceChange)
        {
            Stock currentStock = await this.portfolioContext.Stocks.Where(p => p.StockId == id)
                                           .FirstAsync();
            currentStock.LatestPrice = stockPriceChange;

            await this.portfolioContext.SaveChangesAsync();

            await this.hubContext.Clients.All.SendAsync("UpdatePortfolio");
        }
    }
}
