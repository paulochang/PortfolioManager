namespace statgen.Models
{
    public class PortfolioStockDto
    {
        public PortfolioStockDto(int stockId, int qty)
        {
            StockId = stockId;
            Qty = qty;
        }

        public int StockId { get; }

        public int Qty { get; }
    }
}