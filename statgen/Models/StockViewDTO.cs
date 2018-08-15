using System;

namespace statgen.Models
{
    public class StockViewDto
    {
        private decimal _price;

        public string Symbol { get; set; }

        public decimal Open { get; private set; }

        public decimal Low { get; private set; }

        public decimal High { get; private set; }

        public decimal LastChange { get; private set; }

        public decimal Change => Price - Open;

        public double Perc => (double) Math.Round(Change / Price, 4);

        public decimal Price
        {
            get => _price;
            set
            {
                if (_price == value) return;

                LastChange = value - _price;
                _price = value;

                if (Open == 0) Open = _price;
                if (_price < Low || Low == 0) Low = _price;
                if (_price > High) High = _price;
            }
        }
    }
}