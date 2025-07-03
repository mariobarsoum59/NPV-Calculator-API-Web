namespace NPVCalculator.Data.Models
{
    public class NPVCalculation
    {
        public DiscountRate DiscountRate { get; }
        public Money NetPresentValue { get; }
        public DateTime CalculatedAt { get; }

        public NPVCalculation(DiscountRate discountRate, Money netPresentValue)
        {
            DiscountRate = discountRate ?? throw new ArgumentNullException(nameof(discountRate));
            NetPresentValue = netPresentValue ?? throw new ArgumentNullException(nameof(netPresentValue));
            CalculatedAt = DateTime.UtcNow;
        }
    }
}
