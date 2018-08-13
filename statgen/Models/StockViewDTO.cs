using System;
namespace statgen.Models
{
    public class StockViewDTO
    {

        private decimal _price;

        public string Symbol { get; set; }

        public decimal Open { get; private set; }

        public decimal Low { get; private set; }

        public decimal High { get; private set; }

        public decimal LastChange { get; private set; }

        public decimal Change
        {
            get
            {
                return Price - Open;
            }
        }

        public double Perc
        {
            get
            {
                return (double)Math.Round(Change / Price, 4);
            }
        }

        public decimal Price
        {
            get
            {
                return _price;
            }
            set
            {
                if (_price == value)
                {
                    return;
                }

                LastChange = value - _price;
                _price = value;

                if (Open == 0)
                {
                    Open = _price;
                }
                if (_price < Low || Low == 0)
                {
                    Low = _price;
                }
                if (_price > High)
                {
                    High = _price;
                }
            }
        }
    }
}