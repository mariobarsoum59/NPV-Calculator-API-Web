using NPVCalculator.Data.DataAccess;

namespace NPVCalculatorAPI.Interfaces
{
    public interface INPVCalculationService
    {
        Task<NPVCalculationResultDto> CalculateNPVRangeAsync(NPVCalculationRequestDto request);
    }
}
