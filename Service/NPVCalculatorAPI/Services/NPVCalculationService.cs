using NPVCalculator.Data.DataAccess;
using NPVCalculator.Data.Interfaces;
using NPVCalculator.Data.Models;
using NPVCalculatorAPI.Interfaces;

namespace NPVCalculatorAPI.Services
{
    public class NPVCalculationService : INPVCalculationService
    {
        private readonly INPVCalculator _npvCalculator;
        private readonly ILogger<NPVCalculationService> _logger;

        public NPVCalculationService(INPVCalculator npvCalculator, ILogger<NPVCalculationService> logger)
        {
            _npvCalculator = npvCalculator ?? throw new ArgumentNullException(nameof(npvCalculator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<NPVCalculationResultDto> CalculateNPVRangeAsync(NPVCalculationRequestDto request)
        {
            try
            {
                _logger.LogInformation("Starting NPV calculation for {Count} cash flows", request.CashFlows.Count);

                var cashFlows = request.CashFlows
                    .Select((amount, index) => new CashFlow(index, new Money(amount, request.Currency)))
                    .ToList();

                var lowerBound = DiscountRate.FromPercentage(request.LowerBound);
                var upperBound = DiscountRate.FromPercentage(request.UpperBound);

                var calculations = await Task.Run(() =>
                    _npvCalculator.CalculateRange(cashFlows, lowerBound, upperBound, request.Increment)
                );

                var results = calculations.Select(calc => new NPVResultItemDto
                {
                    DiscountRate = calc.DiscountRate.Value,
                    NPV = calc.NetPresentValue.Amount,
                    FormattedRate = calc.DiscountRate.ToString(),
                    Currency = calc.NetPresentValue.Currency
                }).ToList();

                var metadata = new NPVCalculationMetadataDto
                {
                    CashFlowCount = cashFlows.Count,
                    CalculationCount = results.Count,
                    CalculatedAt = DateTime.UtcNow,
                };

                return new NPVCalculationResultDto
                {
                    Results = results,
                    Metadata = metadata
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating NPV range");
                throw;
            }
        }
    }
}
