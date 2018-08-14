namespace statgen.Models
{
    public class PortfolioComponentViewDTO
    {
        public PortfolioComponentViewDTO(int stockId, int qty, string stockName, double unitValue, double totalValue)
        {
            StockId = stockId;
            Qty = qty;
            StockName = stockName;
            UnitValue = unitValue;
            TotalValue = totalValue;
        }

        public int StockId { get; }

        public int Qty { get; }

        public string StockName { get; }

        public double UnitValue { get; }

        public double TotalValue { get; }
    }
}