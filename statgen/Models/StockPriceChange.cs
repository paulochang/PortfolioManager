using System;
namespace statgen.Models
{
    public class StockPriceChange
    {
        /*
        Symbol: 'MSFT', 
        Price: 23.32
        */              

        public int Index
        {
            get;
            set;
        }

        public decimal Price
        {
            get;
            set;
        }
    }
}
