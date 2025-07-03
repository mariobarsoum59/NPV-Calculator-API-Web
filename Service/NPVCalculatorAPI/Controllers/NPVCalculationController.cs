using Microsoft.AspNetCore.Mvc;
using NPVCalculator.Data.DataAccess;
using NPVCalculatorAPI.Interfaces;
using NPVCalculatorAPI.Models;

namespace NPVCalculatorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NPVCalculationController : ControllerBase
    {
        private readonly INPVCalculationService _calculationService;
        private readonly ILogger<NPVCalculationController> _logger;

        public NPVCalculationController(INPVCalculationService calculationService, ILogger<NPVCalculationController> logger)
        {
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("calculate")]
        [ProducesResponseType(typeof(ApiResponse<NPVCalculationResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CalculateNPVRange([FromBody] NPVCalculationRequestDto request)
        {
            try
            {
                _logger.LogInformation("Received NPV calculation request with {Count} cash flows", request.CashFlows?.Count ?? 0);

                var result = await _calculationService.CalculateNPVRangeAsync(request);

                return Ok(new ApiResponse<NPVCalculationResultDto>
                {
                    Success = true,
                    Data = result,
                    Message = "NPV calculations completed successfully"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in NPV calculation request");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "INVALID_REQUEST",
                        Message = ex.Message
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during NPV calculation");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "INTERNAL_ERROR",
                        Message = "An unexpected error occurred while processing your request"
                    }
                });
            }
        }
    }
}
