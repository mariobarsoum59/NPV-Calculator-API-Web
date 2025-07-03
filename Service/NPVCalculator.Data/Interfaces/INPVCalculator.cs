using NPVCalculator.Data.Models;

namespace NPVCalculator.Data.Interfaces
{
    public interface INPVCalculator
    {
        NPVCalculation Calculate(IEnumerable<CashFlow> cashFlows, DiscountRate discountRate);
        IEnumerable<NPVCalculation> CalculateRange(IEnumerable<CashFlow> cashFlows, DiscountRate lowerBound, DiscountRate upperBound, decimal increment);
    }
}
