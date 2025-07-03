using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NPVCalculator.Data.DataAccess;
using NPVCalculatorAPI.Controllers;
using NPVCalculatorAPI.Interfaces;
using NPVCalculatorAPI.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NPVCalculator.API.Tests.Controller_Tests
{
    [TestClass]
    public class NPVCalculationControllerTests
    {
        private INPVCalculationService? _mockCalculationService;
        private ILogger<NPVCalculationController>? _mockLogger;
        private NPVCalculationController? _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockCalculationService = Substitute.For<INPVCalculationService>();
            _mockLogger = Substitute.For<ILogger<NPVCalculationController>>();
            _controller = new NPVCalculationController(_mockCalculationService, _mockLogger);
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Act
            var controller = new NPVCalculationController(_mockCalculationService, _mockLogger);

            // Assert
            controller.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new NPVCalculationController(null, _mockLogger);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("calculationService");
        }

        [TestMethod]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new NPVCalculationController(_mockCalculationService, null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        #endregion

        #region CalculateNPVRange Tests - Success Scenarios

        [TestMethod]
        public async Task CalculateNPVRange_WithValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -1000, 500, 500, 500 },
                LowerBound = 5m,
                UpperBound = 15m,
                Increment = 1m,
                Currency = "USD"
            };

            var expectedResult = new NPVCalculationResultDto
            {
                Results = new List<NPVResultItemDto>
                {
                    new NPVResultItemDto
                    {
                        DiscountRate = 0.05m,
                        NPV = 361.11m,
                        FormattedRate = "5.00%",
                        Currency = "USD"
                    }
                },
                Metadata = new NPVCalculationMetadataDto
                {
                    CashFlowCount = 4,
                    CalculationCount = 11,
                    CalculatedAt = DateTime.UtcNow
                }
            };

            _mockCalculationService?.CalculateNPVRangeAsync(request)
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _controller.CalculateNPVRange(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.StatusCode.Should().Be(StatusCodes.Status200OK);

            var response = okResult?.Value as ApiResponse<NPVCalculationResultDto>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().BeEquivalentTo(expectedResult);
            response.Message.Should().Be("NPV calculations completed successfully");
            response.Error.Should().BeNull();
        }

        [TestMethod]
        public async Task CalculateNPVRange_EmptyRequest_StillProcesses()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal>(),
                LowerBound = 5m,
                UpperBound = 5m,
                Increment = 1m,
                Currency = "USD"
            };

            var expectedResult = new NPVCalculationResultDto
            {
                Results = new List<NPVResultItemDto>(),
                Metadata = new NPVCalculationMetadataDto
                {
                    CashFlowCount = 0,
                    CalculationCount = 0,
                    CalculatedAt = DateTime.UtcNow
                }
            };

            _mockCalculationService?.CalculateNPVRangeAsync(request)
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _controller.CalculateNPVRange(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult?.Value as ApiResponse<NPVCalculationResultDto>;
            response?.Success.Should().BeTrue();
            response?.Data?.Results.Should().BeEmpty();
        }

        [TestMethod]
        public async Task CalculateNPVRange_LogsInformationOnRequest()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -1000, 300, 300, 300 },
                LowerBound = 5m,
                UpperBound = 10m,
                Increment = 1m,
                Currency = "USD"
            };

            _mockCalculationService?.CalculateNPVRangeAsync(Arg.Any<NPVCalculationRequestDto>())
                .Returns(new NPVCalculationResultDto());

            // Act
            await _controller.CalculateNPVRange(request);

            // Assert
            _mockLogger?.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("Received NPV calculation request with 4 cash flows")),
                null,
                Arg.Any<Func<object, Exception, string>>()
            );
        }

        [TestMethod]
        public async Task CalculateNPVRange_WithNullCashFlows_LogsZeroCount()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = null,
                LowerBound = 5m,
                UpperBound = 10m,
                Increment = 1m,
                Currency = "USD"
            };

            _mockCalculationService?.CalculateNPVRangeAsync(Arg.Any<NPVCalculationRequestDto>())
                .Returns(new NPVCalculationResultDto());

            // Act
            await _controller.CalculateNPVRange(request);

            // Assert
            _mockLogger?.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("Received NPV calculation request with 0 cash flows")),
                null,
                Arg.Any<Func<object, Exception, string>>()
            );
        }

        #endregion

        #region CalculateNPVRange Tests - Error Scenarios

        [TestMethod]
        public async Task CalculateNPVRange_ServiceThrowsArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -1000 },
                LowerBound = 15m,
                UpperBound = 5m, // Invalid: upper < lower
                Increment = 1m,
                Currency = "USD"
            };

            var exception = new ArgumentException("Upper bound must be greater than lower bound");
            _mockCalculationService?.CalculateNPVRangeAsync(request)
                .Throws(exception);

            // Act
            var result = await _controller.CalculateNPVRange(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            var response = badRequestResult?.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Error.Should().NotBeNull();
            response.Error.Code.Should().Be("INVALID_REQUEST");
            response.Error.Message.Should().Be("Upper bound must be greater than lower bound");
            response.Data.Should().BeNull();
        }

        [TestMethod]
        public async Task CalculateNPVRange_ServiceThrowsArgumentException_LogsWarning()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal>(),
                LowerBound = 5m,
                UpperBound = 10m,
                Increment = 0m, // Invalid: zero increment
                Currency = "USD"
            };

            var exception = new ArgumentException("Increment must be positive");
            _mockCalculationService?.CalculateNPVRangeAsync(request)
                .Throws(exception);

            // Act
            await _controller.CalculateNPVRange(request);

            // Assert
            _mockLogger?.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("Invalid argument in NPV calculation request")),
                exception,
                Arg.Any<Func<object, Exception, string>>()
            );
        }

        [TestMethod]
        public async Task CalculateNPVRange_ServiceThrowsGeneralException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -1000, 500 },
                LowerBound = 5m,
                UpperBound = 10m,
                Increment = 1m,
                Currency = "USD"
            };

            var exception = new InvalidOperationException("Database connection failed");
            _mockCalculationService?.CalculateNPVRangeAsync(request)
                .Throws(exception);

            // Act
            var result = await _controller.CalculateNPVRange(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult?.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

            var response = objectResult?.Value as ApiResponse<object>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Error.Should().NotBeNull();
            response.Error.Code.Should().Be("INTERNAL_ERROR");
            response.Error.Message.Should().Be("An unexpected error occurred while processing your request");
        }

        [TestMethod]
        public async Task CalculateNPVRange_ServiceThrowsGeneralException_LogsError()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -1000 },
                LowerBound = 5m,
                UpperBound = 10m,
                Increment = 1m,
                Currency = "USD"
            };

            var exception = new Exception("Unexpected error");
            _mockCalculationService?.CalculateNPVRangeAsync(request)
                .Throws(exception);

            // Act
            await _controller.CalculateNPVRange(request);

            // Assert
            _mockLogger?.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("Unexpected error during NPV calculation")),
                exception,
                Arg.Any<Func<object, Exception, string>>()
            );
        }

        #endregion

        #region Integration and Edge Case Tests

        [TestMethod]
        public async Task CalculateNPVRange_LargeDataSet_HandlesSuccessfully()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000000 };
            for (int i = 0; i < 100; i++)
            {
                cashFlows.Add(15000);
            }

            var request = new NPVCalculationRequestDto
            {
                CashFlows = cashFlows,
                LowerBound = 1m,
                UpperBound = 50m,
                Increment = 0.1m,
                Currency = "USD"
            };

            var expectedResult = new NPVCalculationResultDto
            {
                Results = new List<NPVResultItemDto>(), // Simplified
                Metadata = new NPVCalculationMetadataDto
                {
                    CashFlowCount = 101,
                    CalculationCount = 491,
                    CalculatedAt = DateTime.UtcNow
                }
            };

            _mockCalculationService?.CalculateNPVRangeAsync(request)
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _controller.CalculateNPVRange(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult?.Value as ApiResponse<NPVCalculationResultDto>;
            response?.Data?.Metadata?.CashFlowCount.Should().Be(101);
        }

        [TestMethod]
        public async Task CalculateNPVRange_ServiceReturnsNull_HandlesGracefully()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -1000, 500 },
                LowerBound = 5m,
                UpperBound = 10m,
                Increment = 1m,
                Currency = "USD"
            };

            _mockCalculationService?.CalculateNPVRangeAsync(request)
                .Returns(Task.FromResult<NPVCalculationResultDto>(null));

            // Act
            var result = await _controller.CalculateNPVRange(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult?.Value as ApiResponse<NPVCalculationResultDto>;
            response?.Success.Should().BeTrue();
            response?.Data.Should().BeNull();
        }

        [TestMethod]
        public async Task CalculateNPVRange_ConcurrentRequests_HandledIndependently()
        {
            // Arrange
            var request1 = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -1000, 500 },
                LowerBound = 5m,
                UpperBound = 10m,
                Increment = 1m,
                Currency = "USD"
            };

            var request2 = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -2000, 1000 },
                LowerBound = 10m,
                UpperBound = 20m,
                Increment = 2m,
                Currency = "EUR"
            };

            var result1 = new NPVCalculationResultDto
            {
                Results = new List<NPVResultItemDto> { new NPVResultItemDto { NPV = 100m } },
                Metadata = new NPVCalculationMetadataDto { CashFlowCount = 2 }
            };

            var result2 = new NPVCalculationResultDto
            {
                Results = new List<NPVResultItemDto> { new NPVResultItemDto { NPV = 200m } },
                Metadata = new NPVCalculationMetadataDto { CashFlowCount = 2 }
            };

            _mockCalculationService?.CalculateNPVRangeAsync(request1).Returns(result1);
            _mockCalculationService?.CalculateNPVRangeAsync(request2).Returns(result2);

            // Act
            var task1 = _controller?.CalculateNPVRange(request1);
            var task2 = _controller?.CalculateNPVRange(request2);

            var results = await Task.WhenAll(task1, task2);

            // Assert
            var response1 = ((OkObjectResult)results[0]).Value as ApiResponse<NPVCalculationResultDto>;
            var response2 = ((OkObjectResult)results[1]).Value as ApiResponse<NPVCalculationResultDto>;

            response1?.Data?.Results[0].NPV.Should().Be(100m);
            response2?.Data?.Results[0].NPV.Should().Be(200m);
        }

        #endregion
    }
}
