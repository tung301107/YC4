using YC4.Interfaces;

namespace YC4.Services
{
    public class PriceCalculator : IPriceCalculator
    {
        public decimal CalculateTotal(IEnumerable<decimal> seatPrices)
        {
            return seatPrices.Sum(); // Có thể thêm logic VAT hoặc giảm giá ở đây
        }
    }
}
