namespace YC4.Interfaces
{
    public interface IPriceCalculator
    {
        decimal CalculateTotal(IEnumerable<decimal> seatPrices);
    }
}
