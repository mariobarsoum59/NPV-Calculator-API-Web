using NPVCalculator.Data.Interfaces;
using NPVCalculator.Data.Models;

namespace NPVCalculatorAPI.Services
{
    public class NPVCalculatorService : INPVCalculator
    {
        public NPVCalculation Calculate(IEnumerable<CashFlow> cashFlows, DiscountRate discountRate)
        {
            if (cashFlows == null || !cashFlows.Any())
                throw new ArgumentException("Cash flows cannot be null or empty", nameof(cashFlows));

            if (discountRate == null)
                throw new ArgumentNullException(nameof(discountRate));

            var totalPresentValue = cashFlows
                .Select(cf => cf.GetPresentValue(discountRate))
                .Aggregate((a, b) => a + b);

            return new NPVCalculation(discountRate, totalPresentValue);
        }

        public IEnumerable<NPVCalculation> CalculateRange(IEnumerable<CashFlow> cashFlows, DiscountRate lowerBound, DiscountRate upperBound, decimal increment)
        {
            if (lowerBound == null || upperBound == null)
                throw new ArgumentNullException("Bounds cannot be null");

            if (lowerBound.Value > upperBound.Value)
                throw new ArgumentException("Lower bound must be less than or equal to upper bound");

            if (increment <= 0)
                throw new ArgumentException("Increment must be positive", nameof(increment));

            var cashFlowsList = cashFlows?.ToList() ?? throw new ArgumentNullException(nameof(cashFlows));
            var results = new List<NPVCalculation>();

            var incrementValue = increment / 100;
            var currentRate = lowerBound.Value;

            while (currentRate <= upperBound.Value + 0.0001m)
            {
                var rate = new DiscountRate(Math.Round(currentRate, 4));
                var calculation = Calculate(cashFlowsList, rate);
                results.Add(calculation);
                currentRate += incrementValue;
            }

            return results;
        }
    }
}
