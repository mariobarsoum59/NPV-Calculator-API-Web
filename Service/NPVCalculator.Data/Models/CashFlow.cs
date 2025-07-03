namespace NPVCalculator.Data.Models
{
    public class CashFlow
    {
        public int Period { get; }
        public Money Amount { get; }

        public CashFlow(int period, Money amount)
        {
            if (period < 0)
                throw new ArgumentException("Period cannot be negative", nameof(period));

            Period = period;
            Amount = amount ?? throw new ArgumentNullException(nameof(amount));
        }

        public Money GetPresentValue(DiscountRate discountRate)
        {
            var discountFactor = Math.Pow(1 + (double)discountRate.Value, Period);
            var presentValue = Amount.Amount / (decimal)discountFactor;
            return new Money(Math.Round(presentValue, 2), Amount.Currency);
        }
    }
}
