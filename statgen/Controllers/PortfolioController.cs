using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using statgen.Hubs;
using statgen.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace statgen.Controllers
{
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        readonly IHubContext<StockStatsHub> hubContext;

        public PortfolioController(IHubContext<StockStatsHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        [HttpPost("/price")]
        public Task PostPriceChange(StockPriceChange stockPriceChange){
            return this.hubContext.Clients.All.SendAsync("Update", stockPriceChange);
        }
    }
}
