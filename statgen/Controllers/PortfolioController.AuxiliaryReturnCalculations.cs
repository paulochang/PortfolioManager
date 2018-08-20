using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using statgen.Models;
using statgen.SQLite;

namespace statgen.Controllers
{
    public partial class PortfolioController
    {
        #region Auxiliary Portfolio Methods

        private async Task<Dictionary<int, double>> CreateOrUpdatePortfolioPriceRecord(int stockId, DateTime savingDate)
        {
            var watch = new Stopwatch();
            //Get list of portfolios to update
            List<Portfolio> portfoliosToUpdate = await _portfolioContext.PortfolioAllocations
                .Where(pa => pa.StockId == stockId)
                .Include(p => p.Portfolio)
                .Select(pa => pa.Portfolio)
                .ToListAsync();

            int[] portfolioIdList = portfoliosToUpdate.Select(p => p.PortfolioId).ToArray();
            
            List<PortfolioAllocation> relevantPortfolioAllocations = _portfolioContext.PortfolioAllocations
                .Where(pa => portfolioIdList.Contains(pa.PortfolioId))
                .ToList();
            //Get array of stocks to monitor
            int[] stockList = relevantPortfolioAllocations.Select(p => p.StockId).Distinct().ToArray();
            
            //Get Stock->Price mapping 
            Dictionary<int, double> priceByStockIdDictionary = new Dictionary<int, double>();
            foreach (int currentStockId in stockList)
            {
                double currentStockPrice = await _portfolioContext.PriceRecords
                    .Where(pr => pr.Datetime <= savingDate)
                    .Where(pr => pr.StockId == currentStockId)
                    .OrderByDescending(pr => pr.Datetime)
                    .Select(pr => pr.Price)
                    .FirstAsync();
                priceByStockIdDictionary.Add(currentStockId, currentStockPrice);
            }


            Dictionary<int, double> portfoliosByPrice = new Dictionary<int, double>();
            //submit new portfolio prices and update old ones
            foreach (Portfolio portfolio in portfoliosToUpdate)
            {
                var portfolioTotalPrice =
                    portfolio.PortfolioStocks.Sum(ps => ps.Qty * priceByStockIdDictionary[ps.StockId]);
                
                var existingRecord = await _portfolioContext.PortfolioPriceRecords
                    .Where(pr => pr.Datetime == savingDate && pr.PortfolioId == portfolio.PortfolioId)
                    .FirstOrDefaultAsync();

                if (existingRecord != null)
                {
                    //modify existing record if already exists
                    existingRecord.Price = portfolioTotalPrice;
                    _portfolioContext.PortfolioPriceRecords.Update(existingRecord);
                }
                else
                {
                    //add new price record if it doesn´t
                    _portfolioContext.PortfolioPriceRecords.Add(
                        new PortfolioPriceRecord()
                        {
                            Datetime = savingDate,
                            Price = portfolioTotalPrice,
                            PortfolioId = portfolio.PortfolioId
                        });
                }

                portfoliosByPrice.Add(portfolio.PortfolioId, portfolioTotalPrice);
            }

            return portfoliosByPrice;
        }

        private async Task CalcPortfolioDailyReturn(int portfolioId, double portfolioPrice, DateTime savingDate)
        {
            //get nearest 1 day or older price
            var nearestDailyPrice = await _portfolioContext.PortfolioPriceRecords
                .Where(pr => pr.Datetime <= savingDate.AddDays(-1))
                .Where(pr => pr.PortfolioId == portfolioId)
                .OrderByDescending(pr => pr.Datetime)
                .Select(pr => pr.Price)
                .FirstAsync();

            _portfolioContext.PortfolioDailyReturns.Add(
                new PortfolioDailyReturn()
                {
                    DateTime = savingDate,
                    Return = portfolioPrice - nearestDailyPrice,
                    PortfolioId = portfolioId
                });
        }

        private async Task CalcPortfolioHourReturn(int portfolioId, double portfolioPrice, DateTime savingDate)
        {
            //get nearest 1 hour or older price
            var nearestHourPrice = await _portfolioContext.PortfolioPriceRecords
                .Where(pr => pr.Datetime <= savingDate.AddHours(-1))
                .Where(pr => pr.PortfolioId == portfolioId)
                .OrderByDescending(pr => pr.Datetime)
                .Select(pr => pr.Price)
                .FirstAsync();

            _portfolioContext.PortfolioHourlyReturns.Add(
                new PortfolioHourlyReturn()
                {
                    DateTime = savingDate,
                    Return = portfolioPrice - nearestHourPrice,
                    PortfolioId = portfolioId
                });
        }

        private async Task CalcPortfolioMinuteReturn(int portfolioId, double portfolioPrice, DateTime savingDate)
        {
            var unused = await _portfolioContext.PortfolioPriceRecords
                .Where(pr => pr.Datetime <= savingDate.AddMinutes(-1))
                .Where(pr => pr.PortfolioId == portfolioId)
                .OrderByDescending(pr => pr.Datetime)
                .FirstAsync();

            //get nearest 1 minute or older price 
            var nearestMinutePrice = await _portfolioContext.PortfolioPriceRecords
                .Where(pr => pr.Datetime <= savingDate.AddMinutes(-1))
                .Where(pr => pr.PortfolioId == portfolioId)
                .OrderByDescending(pr => pr.Datetime)
                .Select(pr => pr.Price)
                .FirstAsync();

            _portfolioContext.PortfolioMinuteReturns.Add(
                new PortfolioMinuteReturn()
                {
                    DateTime = savingDate,
                    Return = portfolioPrice - nearestMinutePrice,
                    PortfolioId = portfolioId
                });
        }

        #endregion

        #region Auxiliary Stock Methods

        private async Task CalcStockDailyReturn(int stockId, StockPriceDto stockPriceDto, DateTime savingDate)
        {
            //get nearest 1 day or older price
            var nearestDailyPrice = await _portfolioContext.PriceRecords
                .Where(pr => pr.Datetime <= savingDate.AddDays(-1))
                .Where(pr => pr.StockId == stockId)
                .OrderByDescending(pr => pr.Datetime)
                .Select(pr => pr.Price)
                .FirstAsync();

            _portfolioContext.DailyReturns.Add(
                new StockDailyReturn()
                {
                    DateTime = savingDate,
                    Return = stockPriceDto.Price - nearestDailyPrice,
                    StockId = stockId
                });
        }

        private async Task CalcStockHourReturn(int stockId, StockPriceDto stockPriceDto, DateTime savingDate)
        {
            //get nearest 1 hour or older price
            var nearestHourPrice = await _portfolioContext.PriceRecords
                .Where(pr => pr.Datetime <= savingDate.AddHours(-1))
                .Where(pr => pr.StockId == stockId)
                .OrderByDescending(pr => pr.Datetime)
                .Select(pr => pr.Price)
                .FirstAsync();

            _portfolioContext.HourlyReturns.Add(
                new StockHourlyReturn()
                {
                    DateTime = savingDate,
                    Return = stockPriceDto.Price - nearestHourPrice,
                    StockId = stockId
                });
        }

        private async Task CalcStockMinuteReturn(int stockId, StockPriceDto stockPriceDto, DateTime savingDate)
        {
            //get nearest 1 minute or older price 
            var nearestMinutePrice = await _portfolioContext.PriceRecords
                .Where(pr => pr.Datetime <= savingDate.AddMinutes(-1))
                .Where(pr => pr.StockId == stockId)
                .OrderByDescending(pr => pr.Datetime)
                .Select(pr => pr.Price)
                .FirstAsync();

            _portfolioContext.MinuteReturns.Add(
                new StockMinuteReturn()
                {
                    DateTime = savingDate,
                    Return = stockPriceDto.Price - nearestMinutePrice,
                    StockId = stockId
                });
        }

        private async Task CreateStockPriceRecord(int stockId, StockPriceDto stockPriceDto, DateTime savingDate)
        {
            // check if record with current date and stock already exists
            var existingRecord = await _portfolioContext.PriceRecords
                .Where(pr => pr.Datetime == savingDate && pr.StockId == stockId)
                .FirstOrDefaultAsync();

            if (existingRecord != null)
            {
                //modify existing record if already exists
                existingRecord.Price = stockPriceDto.Price;
                _portfolioContext.PriceRecords.Update(existingRecord);
            }
            else
            {
                //add new price record if it doesn´t
                _portfolioContext.PriceRecords.Add(
                    new StockPriceRecord()
                    {
                        Datetime = savingDate,
                        Price = stockPriceDto.Price,
                        StockId = stockId
                    });
            }
        }

        #endregion
    }
}