namespace statgen.Models
{
    public class RiskStatsViewDto
    {
        public RiskStatsViewDto(
            double valueAtRisk90, double expectedShortfall90,
            double valueAtRisk95, double expectedShortfall95,
            double valueAtRisk99, double expectedShortfall99)
        {
            ValueAtRisk90 = valueAtRisk90;
            ExpectedShortfall90 = expectedShortfall90;
            ValueAtRisk95 = valueAtRisk95;
            ExpectedShortfall95 = expectedShortfall95;
            ValueAtRisk99 = valueAtRisk99;
            ExpectedShortfall99 = expectedShortfall99;
        }

        // 90-confidence level
        public double ValueAtRisk90 { get; }
        public double ExpectedShortfall90 { get; }

        // 95-confidence level
        public double ValueAtRisk95 { get; }
        public double ExpectedShortfall95 { get; }

        // 99-confidence level
        public double ValueAtRisk99 { get; }
        public double ExpectedShortfall99 { get; }
    }

    public class PortfolioRiskStatsViewDto : RiskStatsViewDto
    {
        public PortfolioRiskStatsViewDto(
            int portfolioId,
            double valueAtRisk90, double expectedShortfall90,
            double valueAtRisk95, double expectedShortfall95,
            double valueAtRisk99, double expectedShortfall99
        ) : base(
            valueAtRisk90, expectedShortfall90,
            valueAtRisk95, expectedShortfall95,
            valueAtRisk99, expectedShortfall99
        )
        {
            PortfolioId = portfolioId;
        }

        public PortfolioRiskStatsViewDto(
            int portfolioId,
            RiskStatsViewDto riskStatsViewDto
        ) : base(
            riskStatsViewDto.ValueAtRisk90, riskStatsViewDto.ExpectedShortfall90,
            riskStatsViewDto.ValueAtRisk95, riskStatsViewDto.ExpectedShortfall95,
            riskStatsViewDto.ValueAtRisk99, riskStatsViewDto.ExpectedShortfall99
        )
        {
            PortfolioId = portfolioId;
        }

        public int PortfolioId { get; }
    }

    public class StockRiskStatsViewDto : RiskStatsViewDto
    {
        public StockRiskStatsViewDto(
            int stockId, string stockName,
            RiskStatsViewDto riskStatsViewDto
        ) : base(
            riskStatsViewDto.ValueAtRisk90, riskStatsViewDto.ExpectedShortfall90,
            riskStatsViewDto.ValueAtRisk95, riskStatsViewDto.ExpectedShortfall95,
            riskStatsViewDto.ValueAtRisk99, riskStatsViewDto.ExpectedShortfall99
        )
        {
            StockId = stockId;
            StockName = stockName;
        }

        public StockRiskStatsViewDto(
            int stockId, string stockName,
            double valueAtRisk90, double expectedShortfall90,
            double valueAtRisk95, double expectedShortfall95,
            double valueAtRisk99, double expectedShortfall99
        ) : base(
            valueAtRisk90, expectedShortfall90,
            valueAtRisk95, expectedShortfall95,
            valueAtRisk99, expectedShortfall99
        )
        {
            StockId = stockId;
            StockName = stockName;
        }

        public int StockId { get; }
        public string StockName { get; }
    }
}