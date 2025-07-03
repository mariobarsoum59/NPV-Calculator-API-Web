using FluentAssertions;
using Microsoft.Extensions.Logging;
using NPVCalculator.Data.DataAccess;
using NPVCalculator.Data.Interfaces;
using NPVCalculator.Data.Models;
using NPVCalculatorAPI.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NPVCalculator.API.Tests.Service_Tests
{
    [TestClass]
    public class NPVCalculationServiceTests
    {
        private INPVCalculator? _mockNPVCalculator;
        private ILogger<NPVCalculationService>? _mockLogger;
        private NPVCalculationService? _service;

        [TestInitialize]
        public void Setup()
        {
            _mockNPVCalculator = Substitute.For<INPVCalculator>();
            _mockLogger = Substitute.For<ILogger<NPVCalculationService>>();
            _service = new NPVCalculationService(_mockNPVCalculator, _mockLogger);
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Act
            var service = new NPVCalculationService(_mockNPVCalculator, _mockLogger);

            // Assert
            service.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithNullCalculator_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new NPVCalculationService(null, _mockLogger);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("npvCalculator");
        }

        [TestMethod]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new NPVCalculationService(_mockNPVCalculator, null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        #endregion

        #region CalculateNPVRangeAsync Tests

        [TestMethod]
        public async Task CalculateNPVRangeAsync_WithValidRequest_ReturnsCorrectResult()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -1000, 300, 300, 300, 300, 300 },
                LowerBound = 1m,
                UpperBound = 15m,
                Increment = 7m,
                Currency = "USD"
            };

            var mockCalculations = new List<NPVCalculation>
            {
                new NPVCalculation(new DiscountRate(0.01m), new Money(481.44m, "USD")),
                new NPVCalculation(new DiscountRate(0.08m), new Money(206.17m, "USD")),
                new NPVCalculation(new DiscountRate(0.15m), new Money(6.72m, "USD"))
            };

            _mockNPVCalculator?.CalculateRange(
                Arg.Any<IEnumerable<CashFlow>>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<decimal>()
            ).Returns(mockCalculations);

            // Act
            var result = await _service.CalculateNPVRangeAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Results.Should().HaveCount(3);
            result?.Metadata?.CashFlowCount.Should().Be(6);
            result?.Metadata?.CalculationCount.Should().Be(3);
            result?.Metadata?.CalculatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            // Verify first result
            result?.Results[0].DiscountRate.Should().Be(0.01m);
            result.Results[0].NPV.Should().Be(481.44m);
            result.Results[0].FormattedRate.Should().Be("1.00%");
            result.Results[0].Currency.Should().Be("USD");

            // Verify logging - Fixed version
            _mockLogger?.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("Starting NPV calculation for") && o.ToString().Contains("6")),
                null,
                Arg.Any<Func<object, Exception, string>>()
            );
        }

        [TestMethod]
        public async Task CalculateNPVRangeAsync_CashFlowsAreMappedCorrectly()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -5000, 1000, 2000, 3000 },
                LowerBound = 5m,
                UpperBound = 5m,
                Increment = 1m,
                Currency = "EUR"
            };

            List<CashFlow> capturedCashFlows = null;
            _mockNPVCalculator?.CalculateRange(
                Arg.Do<IEnumerable<CashFlow>>(cf => capturedCashFlows = cf.ToList()),
                Arg.Any<DiscountRate>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<decimal>()
            ).Returns(new List<NPVCalculation>
            {
                new NPVCalculation(new DiscountRate(0.05m), new Money(100m, "EUR"))
            });

            // Act
            await _service?.CalculateNPVRangeAsync(request);

            // Assert
            capturedCashFlows.Should().NotBeNull();
            capturedCashFlows.Should().HaveCount(4);

            capturedCashFlows[0].Period.Should().Be(0);
            capturedCashFlows[0].Amount.Amount.Should().Be(-5000m);
            capturedCashFlows[0].Amount.Currency.Should().Be("EUR");

            capturedCashFlows[1].Period.Should().Be(1);
            capturedCashFlows[1].Amount.Amount.Should().Be(1000m);

            capturedCashFlows[2].Period.Should().Be(2);
            capturedCashFlows[2].Amount.Amount.Should().Be(2000m);

            capturedCashFlows[3].Period.Should().Be(3);
            capturedCashFlows[3].Amount.Amount.Should().Be(3000m);
        }

        [TestMethod]
        public async Task CalculateNPVRangeAsync_DiscountRatesAreMappedCorrectly()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -1000, 1200 },
                LowerBound = 5m,    // 5%
                UpperBound = 10m,   // 10%
                Increment = 2.5m,
                Currency = "USD"
            };

            DiscountRate capturedLowerBound = null;
            DiscountRate capturedUpperBound = null;
            decimal capturedIncrement = 0;

            _mockNPVCalculator?.CalculateRange(
                Arg.Any<IEnumerable<CashFlow>>(),
                Arg.Do<DiscountRate>(lb => capturedLowerBound = lb),
                Arg.Do<DiscountRate>(ub => capturedUpperBound = ub),
                Arg.Do<decimal>(inc => capturedIncrement = inc)
            ).Returns(new List<NPVCalculation>());

            // Act
            await _service?.CalculateNPVRangeAsync(request);

            // Assert
            capturedLowerBound?.Value.Should().Be(0.05m);  // 5% as decimal
            capturedUpperBound?.Value.Should().Be(0.10m);  // 10% as decimal
            capturedIncrement.Should().Be(2.5m);
        }

        [TestMethod]
        public async Task CalculateNPVRangeAsync_ResultsAreMappedCorrectly()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -1000, 600, 600 },
                LowerBound = 5m,
                UpperBound = 15m,
                Increment = 5m,
                Currency = "GBP"
            };

            var mockCalculations = new List<NPVCalculation>
            {
                new NPVCalculation(new DiscountRate(0.05m), new Money(142.86m, "GBP")),
                new NPVCalculation(new DiscountRate(0.10m), new Money(41.32m, "GBP")),
                new NPVCalculation(new DiscountRate(0.15m), new Money(-48.49m, "GBP"))
            };

            _mockNPVCalculator?.CalculateRange(
                Arg.Any<IEnumerable<CashFlow>>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<decimal>()
            ).Returns(mockCalculations);

            // Act
            var result = await _service.CalculateNPVRangeAsync(request);

            // Assert
            result.Results.Should().HaveCount(3);

            // Verify all results are mapped correctly
            for (int i = 0; i < mockCalculations.Count; i++)
            {
                result.Results[i].DiscountRate.Should().Be(mockCalculations[i].DiscountRate.Value);
                result.Results[i].NPV.Should().Be(mockCalculations[i].NetPresentValue.Amount);
                result.Results[i].FormattedRate.Should().Be(mockCalculations[i].DiscountRate.ToString());
                result.Results[i].Currency.Should().Be(mockCalculations[i].NetPresentValue.Currency);
            }
        }

        [TestMethod]
        public async Task CalculateNPVRangeAsync_EmptyCashFlows_ReturnsEmptyResults()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal>(),
                LowerBound = 5m,
                UpperBound = 10m,
                Increment = 1m,
                Currency = "USD"
            };

            _mockNPVCalculator?.CalculateRange(
                Arg.Any<IEnumerable<CashFlow>>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<decimal>()
            ).Returns(new List<NPVCalculation>());

            // Act
            var result = await _service?.CalculateNPVRangeAsync(request);

            // Assert
            result.Results.Should().BeEmpty();
            result?.Metadata?.CashFlowCount.Should().Be(0);
            result?.Metadata?.CalculationCount.Should().Be(0);
        }

        [TestMethod]
        public async Task CalculateNPVRangeAsync_WhenCalculatorThrows_LogsErrorAndRethrows()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -1000, 500, 500 },
                LowerBound = 5m,
                UpperBound = 10m,
                Increment = 1m,
                Currency = "USD"
            };

            var expectedException = new InvalidOperationException("Calculation failed");

            _mockNPVCalculator?.CalculateRange(
                Arg.Any<IEnumerable<CashFlow>>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<decimal>()
            ).Throws(expectedException);

            // Act
            Func<Task> act = async () => await _service.CalculateNPVRangeAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Calculation failed");

            // Verify error was logged
            _mockLogger?.Received(1).LogError(
                expectedException,
                "Error calculating NPV range"
            );
        }

        [TestMethod]
        public async Task CalculateNPVRangeAsync_TaskIsExecutedAsynchronously()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -1000, 1100 },
                LowerBound = 5m,
                UpperBound = 10m,
                Increment = 5m,
                Currency = "USD"
            };

            var tcs = new TaskCompletionSource<IEnumerable<NPVCalculation>>();

            _mockNPVCalculator?.CalculateRange(
                Arg.Any<IEnumerable<CashFlow>>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<decimal>()
            ).Returns(callInfo => tcs.Task.Result);

            // Set result after a delay to simulate async operation
            _ = Task.Run(async () =>
            {
                await Task.Delay(100);
                tcs.SetResult(new List<NPVCalculation>
                {
                    new NPVCalculation(new DiscountRate(0.05m), new Money(47.62m, "USD"))
                });
            });

            // Act
            var resultTask = _service.CalculateNPVRangeAsync(request);

            // Assert - task should not be completed immediately
            resultTask.IsCompleted.Should().BeFalse();

            // Wait for completion
            var result = await resultTask;
            result.Should().NotBeNull();
            result.Results.Should().HaveCount(1);
        }

        [TestMethod]
        public async Task CalculateNPVRangeAsync_MetadataCalculatedAtIsUtc()
        {
            // Arrange
            var request = new NPVCalculationRequestDto
            {
                CashFlows = new List<decimal> { -1000, 1100 },
                LowerBound = 10m,
                UpperBound = 10m,
                Increment = 1m,
                Currency = "USD"
            };

            _mockNPVCalculator?.CalculateRange(
                Arg.Any<IEnumerable<CashFlow>>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<decimal>()
            ).Returns(new List<NPVCalculation>
            {
                new NPVCalculation(new DiscountRate(0.1m), new Money(0m, "USD"))
            });

            // Act
            var result = await _service.CalculateNPVRangeAsync(request);

            // Assert
            result?.Metadata?.CalculatedAt.Kind.Should().Be(DateTimeKind.Utc);
            result?.Metadata?.CalculatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        #endregion

        #region Integration Scenarios

        [TestMethod]
        public async Task CalculateNPVRangeAsync_LargeDataSet_HandlesCorrectly()
        {
            // Arrange
            var cashFlows = new List<decimal> { -100000 };
            for (int i = 1; i <= 50; i++)
            {
                cashFlows.Add(3000);
            }

            var request = new NPVCalculationRequestDto
            {
                CashFlows = cashFlows,
                LowerBound = 1m,
                UpperBound = 5m,
                Increment = 0.1m,
                Currency = "USD"
            };

            // Create 41 mock calculations (1% to 5% with 0.1% increment)
            var mockCalculations = new List<NPVCalculation>();
            for (decimal rate = 0.01m; rate <= 0.05m; rate += 0.001m)
            {
                mockCalculations.Add(new NPVCalculation(
                    new DiscountRate(rate),
                    new Money(1000m, "USD") // Simplified NPV
                ));
            }

            _mockNPVCalculator?.CalculateRange(
                Arg.Any<IEnumerable<CashFlow>>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<DiscountRate>(),
                Arg.Any<decimal>()
            ).Returns(mockCalculations);

            // Act
            var result = await _service.CalculateNPVRangeAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Metadata?.CashFlowCount.Should().Be(51);
            result.Results.Count.Should().Be(mockCalculations.Count);
        }

        [TestMethod]
        public async Task CalculateNPVRangeAsync_DifferentCurrencies_PreservesCurrency()
        {
            // Arrange
            var currencies = new[] { "USD", "EUR", "GBP", "JPY", "CHF" };

            foreach (var currency in currencies)
            {
                var request = new NPVCalculationRequestDto
                {
                    CashFlows = new List<decimal> { -1000, 1100 },
                    LowerBound = 5m,
                    UpperBound = 5m,
                    Increment = 1m,
                    Currency = currency
                };

                _mockNPVCalculator?.CalculateRange(
                    Arg.Any<IEnumerable<CashFlow>>(),
                    Arg.Any<DiscountRate>(),
                    Arg.Any<DiscountRate>(),
                    Arg.Any<decimal>()
                ).Returns(new List<NPVCalculation>
                {
                    new NPVCalculation(new DiscountRate(0.05m), new Money(47.62m, currency))
                });

                // Act
                var result = await _service.CalculateNPVRangeAsync(request);

                // Assert
                result.Results.Should().AllSatisfy(r => r.Currency.Should().Be(currency));
            }
        }

        #endregion
    }
}
