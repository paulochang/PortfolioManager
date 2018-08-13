using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using statgen.Hubs;
using statgen.Models;
using statgen.SQLite;
using System.Linq;

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
        public Task PostPriceChange(StockPriceChange stockPriceChange){
            return this.hubContext.Clients.All.SendAsync("Update", stockPriceChange);
        }

        [HttpGet("/portfolio/{id}")]
        public ActionResult<IEnumerable<PortfolioComponentViewDTO>> Get(int id)
        {
            List<PortfolioComponentViewDTO> result = this.portfolioContext.PortfolioAllocations
                                                         .Where(p => p.PortfolioId == id)
                                                         .Select(p =>
                                                                 new PortfolioComponentViewDTO() {                                                                
                                                                    Qty = p.Qty,
                                                                    StockName = p.Stock.Name,
                                                                    UnitValue = p.Stock.LatestPrice,
                                                                    TotalValue = p.Stock.LatestPrice * p.Qty
                                                                }
                                                            ).ToList();
            return result;
        }

    }
}
