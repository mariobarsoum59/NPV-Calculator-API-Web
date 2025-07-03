namespace NPVCalculator.Data.DataAccess
{
    public class NPVCalculationRequestDto
    {
        public List<decimal> CashFlows { get; set; } = new();
        public decimal LowerBound { get; set; }
        public decimal UpperBound { get; set; }
        public decimal Increment { get; set; }
        public string Currency { get; set; } = "USD";
    }
}
