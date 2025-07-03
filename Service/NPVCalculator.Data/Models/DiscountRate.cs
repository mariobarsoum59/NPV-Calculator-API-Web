namespace NPVCalculator.Data.Models
{
    public class DiscountRate
    {
        private readonly decimal _value;

        public decimal Value => _value;
        public decimal Percentage => _value * 100;

        public DiscountRate(decimal value)
        {
            if (value < -1)
                throw new ArgumentException("Discount rate cannot be less than -100%", nameof(value));

            _value = value;
        }

        public static DiscountRate FromPercentage(decimal percentage)
        {
            return new DiscountRate(percentage / 100);
        }

        public override string ToString() => $"{Percentage:F2}%";
    }
}
