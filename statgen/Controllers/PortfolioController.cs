using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using statgen.Hubs;
using statgen.Models;
using statgen.SQLite;
using statgen.Utils;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace statgen.Controllers
{
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly IHubContext<StockStatsHub> _hubContext;
        private readonly PortfolioContext _portfolioContext;

        public PortfolioController(IHubContext<StockStatsHub> hubContext, PortfolioContext portfolioContext)
        {
            _hubContext = hubContext;
            _portfolioContext = portfolioContext;
        }

        [HttpGet("/portfolio/{id}")]
        public async Task<ActionResult<IEnumerable<PortfolioComponentViewDto>>> GetPortfolioInfo(int id)
        {
            return await _portfolioContext.PortfolioAllocations
                .Where(p => p.PortfolioId == id)
                .Select(p =>
                    new PortfolioComponentViewDto(
                        p.StockId,
                        p.Qty,
                        p.Stock.Name,
                        p.Stock.LatestPrice,
                        p.Stock.LatestPrice * p.Qty
                    )
                ).ToListAsync();
        }

        [HttpPut("/stock/{id}/price")]
        public async Task PutStockPriceChange(int id, [FromBody] double stockPriceChange)
        {
            var currentStock = await _portfolioContext.Stocks.Where(p => p.StockId == id)
                .FirstAsync();
            currentStock.LatestPrice = stockPriceChange;

            await _portfolioContext.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("UpdatePortfolio");
        }

        [HttpGet("/portfolio/{id}/risk/minute")]
        public async Task<ActionResult<PortfolioRiskStatsViewDto>> GetPortfolioMinuteRiskInfo(int id)
        {
            List<double> fullReturnSet = await _portfolioContext.PortfolioMinuteReturns
                .Where(p => p.PortfolioId == id)
                .Where(p => p.DateTime < new DateTime(2018, 08, 01) && p.DateTime >= new DateTime(2018, 07, 01))
                .OrderBy(p => p.Return)
                .Select(p => p.Return)
                .ToListAsync();

            var partialResult = await RiskCalculator.GetAssetRiskStats(fullReturnSet);

            PortfolioRiskStatsViewDto result = new PortfolioRiskStatsViewDto(
                id, partialResult);

            return result;
        }

        [HttpGet("/portfolio/{id}/risk/hour")]
        public async Task<ActionResult<PortfolioRiskStatsViewDto>> GetPortfolioHourlyRiskInfo(int id)
        {
            List<double> fullReturnSet = await _portfolioContext.PortfolioHourlyReturns
                .Where(p => p.PortfolioId == id)
                .Where(p => p.DateTime < new DateTime(2018, 08, 01) && p.DateTime >= new DateTime(2018, 07, 01))
                .OrderBy(p => p.Return)
                .Select(p => p.Return)
                .ToListAsync();

            var partialResult = await RiskCalculator.GetAssetRiskStats(fullReturnSet);

            PortfolioRiskStatsViewDto result = new PortfolioRiskStatsViewDto(
                id, partialResult);

            return result;
        }

        [HttpGet("/portfolio/{id}/risk/day")]
        public async Task<ActionResult<PortfolioRiskStatsViewDto>> GetPortfolioDailyRiskInfo(int id)
        {
            List<double> fullReturnSet = await _portfolioContext.PortfolioDailyReturns
                .Where(p => p.PortfolioId == id)
                .Where(p => p.DateTime < new DateTime(2018, 08, 01) && p.DateTime >= new DateTime(2018, 07, 01))
                .OrderBy(p => p.Return)
                .Select(p => p.Return)
                .ToListAsync();

            var partialResult = await RiskCalculator.GetAssetRiskStats(fullReturnSet);

            PortfolioRiskStatsViewDto result = new PortfolioRiskStatsViewDto(
                id, partialResult);

            return result;
        }
    }
}