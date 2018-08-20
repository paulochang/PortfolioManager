using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using statgen.Hubs;
using statgen.Models;
using statgen.SQLite;
using statgen.Utils;

namespace statgen.Controllers
{
    [ApiController]
    public partial class PortfolioController : ControllerBase
    {
        private readonly IHubContext<StockStatsHub> _hubContext;
        private readonly PortfolioContext _portfolioContext;
        private readonly ILogger<PortfolioController> _logger;

        public PortfolioController(IHubContext<StockStatsHub> hubContext, PortfolioContext portfolioContext,
            ILogger<PortfolioController> logger)
        {
            _hubContext = hubContext;
            _portfolioContext = portfolioContext;
            _logger = logger;
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

        [HttpPost("/stock/{stockid}/price")]
        public async Task PutStockPriceChange(int stockid, StockPriceDto stockPriceDto)
        {
            
            //locate stock information 
            var currentStock = await _portfolioContext.Stocks
                .Where(p => p.StockId == stockid)
                .FirstAsync();

            // format date (strip away seconds, milliseconds, etc)
            var baseDate = stockPriceDto.DateTime;
            var savingDate = new DateTime(
                baseDate.Year,
                baseDate.Month,
                baseDate.Day,
                baseDate.Hour,
                baseDate.Minute,
                0, DateTimeKind.Local);

            // if current stock price is more recent then ...
            // update the .. update time and latest price information 
            if (currentStock.LastUpdated <= savingDate)
            {
                currentStock.LatestPrice = stockPriceDto.Price;
                currentStock.LastUpdated = savingDate;
            }

            await CreateStockPriceRecord(stockid, stockPriceDto, savingDate);

            await CalcStockMinuteReturn(stockid, stockPriceDto, savingDate);

            await CalcStockHourReturn(stockid, stockPriceDto, savingDate);

            await CalcStockDailyReturn(stockid, stockPriceDto, savingDate);
            
            await _portfolioContext.SaveChangesAsync();

            Dictionary<int, double> PortfoliosByPrice = await CreateOrUpdatePortfolioPriceRecord(stockid, savingDate);

            foreach (var portfolioPrice in PortfoliosByPrice)
            {
                var dailyReturnTask = CalcPortfolioDailyReturn(portfolioPrice.Key, portfolioPrice.Value, savingDate);
                var hourReturnTask = CalcPortfolioHourReturn(portfolioPrice.Key, portfolioPrice.Value, savingDate);
                var minuteReturnTask = CalcPortfolioMinuteReturn(portfolioPrice.Key, portfolioPrice.Value, savingDate);

                Task.WaitAll(dailyReturnTask, hourReturnTask, minuteReturnTask);
            }

            await _portfolioContext.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("UpdatePortfolio");
        }

        [HttpGet("/portfolio/{id}/risk/minute")]
        public async Task<ActionResult<PortfolioRiskStatsViewDto>> GetPortfolioMinuteRiskInfo(int id)
        {
            DateTime baseDate = DateTime.Now;
#if DEBUG
            baseDate = new DateTime(2018, 08, 01);
#endif

            List<double> fullReturnSet = await _portfolioContext.PortfolioMinuteReturns
                .Where(p => p.PortfolioId == id)
                .Where(p => p.DateTime < baseDate && p.DateTime >= baseDate.AddMonths(-1))
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
            DateTime baseDate = DateTime.Now;
#if DEBUG
            baseDate = new DateTime(2018, 08, 01);
#endif

            List<double> fullReturnSet = await _portfolioContext.PortfolioHourlyReturns
                .Where(p => p.PortfolioId == id)
                .Where(p => p.DateTime < baseDate && p.DateTime >= baseDate.AddMonths(-1))
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
            DateTime baseDate = DateTime.Now;
#if DEBUG
            baseDate = new DateTime(2018, 08, 01);
#endif

            List<double> fullReturnSet = await _portfolioContext.PortfolioDailyReturns
                .Where(p => p.PortfolioId == id)
                .Where(p => p.DateTime < baseDate && p.DateTime >= baseDate.AddMonths(-1))
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