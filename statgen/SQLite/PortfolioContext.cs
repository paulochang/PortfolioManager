using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace statgen.SQLite
{
    public class PortfolioContext : DbContext
    {
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<PortfolioAllocation> PortfolioAllocations { get; set; }
        public DbSet<StockPriceRecord> PriceRecords { get; set; }
        public DbSet<StockMinuteReturn> MinuteReturns { get; set; }
        public DbSet<StockDailyReturn> DailyReturns { get; set; }
        public DbSet<StockHourlyReturn> HourlyReturns { get; set; }
        public DbSet<PortfolioPriceRecord> PortfolioPriceRecords { get; set; }
        public DbSet<PortfolioMinuteReturn> PortfolioMinuteReturns { get; set; }
        public DbSet<PortfolioDailyReturn> PortfolioDailyReturns { get; set; }
        public DbSet<PortfolioHourlyReturn> PortfolioHourlyReturns { get; set; }

        public PortfolioContext(DbContextOptions<PortfolioContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=portfolio.db");
        }
    }

    public class Portfolio
    {
        public int PortfolioId { get; set; }
        public string Owner { get; set; }
        public ICollection<PortfolioAllocation> PortfolioStocks { get; set; }

    }

    public class Stock
    {
        public int StockId { get; set; }
        public string Name { get; set; }
        public double LatestPrice { get; set; }
        public ICollection<PortfolioAllocation> PortfolioAllocations { get; set; }
        public ICollection<StockPriceRecord> StockPriceRecords { get; set; }
        public ICollection<StockMinuteReturn> StockMinuteReturns { get; set; }
        public ICollection<StockDailyReturn> StockDailyReturns { get; set; }
        public ICollection<StockHourlyReturn> StockHourlyReturns { get; set; }
        public ICollection<PortfolioPriceRecord> PortfolioPriceRecords { get; set; }
        public ICollection<PortfolioMinuteReturn> PortfolioMinuteReturns { get; set; }
        public ICollection<PortfolioDailyReturn> PortfolioDailyReturns { get; set; }
        public ICollection<PortfolioHourlyReturn> PortfolioHourlyReturns { get; set; }
    }

    public class PortfolioAllocation
    {
        public int Id { get; set; }
        public int PortfolioId { get; set; }
        public Portfolio Portfolio { get; set; }
        public int StockId { get; set; }
        public Stock Stock { get; set; }
        public int Qty { get; set; }
    }

    public class StockPriceRecord
    {
        public int Id { get; set; }
        public DateTime Datetime { get; set; }
        public int StockId { get; set; }
        public Stock Stock { get; set; }
        public double Price { get; set; }
    }

    public class StockReturn
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int StockId { get; set; }
        public Stock Stock { get; set; }
        public double? Return { get; set; }
    }

    public class StockMinuteReturn : StockReturn { };

    public class StockDailyReturn : StockReturn { };

    public class StockHourlyReturn : StockReturn { };

    public class PortfolioPriceRecord
    {
        public int Id { get; set; }
        public DateTime Datetime { get; set; }
        public int PortfolioId { get; set; }
        public Portfolio Portfolio { get; set; }
        public double Price { get; set; }
    }

    public class PortfolioReturn
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int PortfolioId { get; set; }
        public Portfolio Portfolio { get; set; }
        public double Return { get; set; }
    }

    public class PortfolioMinuteReturn : PortfolioReturn { };

    public class PortfolioDailyReturn : PortfolioReturn { };

    public class PortfolioHourlyReturn : PortfolioReturn { };
}
