namespace NPVCalculator.Data.DataAccess
{
    public class NPVCalculationResultDto
    {
        public List<NPVResultItemDto> Results { get; set; } = new();
        public NPVCalculationMetadataDto? Metadata { get; set; }
    }
}
