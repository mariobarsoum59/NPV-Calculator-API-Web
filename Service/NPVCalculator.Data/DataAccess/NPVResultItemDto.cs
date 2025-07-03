namespace NPVCalculator.Data.DataAccess
{
    public class NPVResultItemDto
    {
        public decimal DiscountRate { get; set; }
        public decimal NPV { get; set; }
        public string? FormattedRate { get; set; }
        public string? Currency { get; set; }
    }
}
