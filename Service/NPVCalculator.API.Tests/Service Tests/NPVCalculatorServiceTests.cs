using FluentAssertions;
using NPVCalculator.Data.Models;
using NPVCalculatorAPI.Services;

namespace NPVCalculator.API.Tests
{
    [TestClass]
    public class NPVCalculatorServiceTests
    {
        private NPVCalculatorService? _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new NPVCalculatorService();
        }

        #region Calculate Method Tests

        [TestMethod]
        public void Calculate_WithValidInputs_ReturnsCorrectNPV()
        {
            // Arrange
            var cashFlows = new List<CashFlow>
            {
                new CashFlow(0, new Money(-1000m, "USD")),
                new CashFlow(1, new Money(300m, "USD")),
                new CashFlow(2, new Money(300m, "USD")),
                new CashFlow(3, new Money(300m, "USD")),
                new CashFlow(4, new Money(300m, "USD")),
                new CashFlow(5, new Money(300m, "USD"))
            };
            var discountRate = new DiscountRate(0.1m);

            // Act
            var result = _service?.Calculate(cashFlows, discountRate);

            // Assert
            result.Should().NotBeNull();
            result.DiscountRate.Should().Be(discountRate);
            result.NetPresentValue.Amount.Should().BeApproximately(137.24m, 0.01m);
            result.NetPresentValue.Currency.Should().Be("USD");
        }

        [TestMethod]
        public void Calculate_WithZeroDiscountRate_ReturnsSumOfCashFlows()
        {
            // Arrange
            var cashFlows = new List<CashFlow>
            {
                new CashFlow(0, new Money(-1000m, "USD")),
                new CashFlow(1, new Money(500m, "USD")),
                new CashFlow(2, new Money(500m, "USD")),
                new CashFlow(3, new Money(500m, "USD"))
            };
            var discountRate = new DiscountRate(0m);

            // Act
            var result = _service?.Calculate(cashFlows, discountRate);

            // Assert
            result?.NetPresentValue.Amount.Should().Be(500m);
        }

        [TestMethod]
        public void Calculate_WithNullCashFlows_ThrowsArgumentException()
        {
            // Arrange
            IEnumerable<CashFlow> cashFlows = null;
            var discountRate = new DiscountRate(0.1m);

            // Act
            Action act = () => _service?.Calculate(cashFlows, discountRate);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Cash flows cannot be null or empty*")
                .WithParameterName("cashFlows");
        }

        [TestMethod]
        public void Calculate_WithEmptyCashFlows_ThrowsArgumentException()
        {
            // Arrange
            var cashFlows = new List<CashFlow>();
            var discountRate = new DiscountRate(0.1m);

            // Act
            Action act = () => _service?.Calculate(cashFlows, discountRate);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Cash flows cannot be null or empty*")
                .WithParameterName("cashFlows");
        }

        [TestMethod]
        public void Calculate_WithNullDiscountRate_ThrowsArgumentNullException()
        {
            // Arrange
            var cashFlows = new List<CashFlow>
            {
                new CashFlow(0, new Money(1000m, "USD"))
            };
            DiscountRate discountRate = null;

            // Act
            Action act = () => _service?.Calculate(cashFlows, discountRate);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("discountRate");
        }

        [TestMethod]
        public void Calculate_WithNegativeCashFlows_CalculatesCorrectly()
        {
            // Arrange
            var cashFlows = new List<CashFlow>
            {
                new CashFlow(0, new Money(-1000m, "USD")),
                new CashFlow(1, new Money(-200m, "USD")),
                new CashFlow(2, new Money(500m, "USD")),
                new CashFlow(3, new Money(800m, "USD")),
                new CashFlow(4, new Money(1000m, "USD"))
            };
            var discountRate = new DiscountRate(0.05m);

            // Act
            var result = _service?.Calculate(cashFlows, discountRate);

            // Assert
            result.Should().NotBeNull();
            // Let's calculate the expected value manually:
            // PV = -1000 + (-200/1.05) + (500/1.05²) + (800/1.05³) + (1000/1.05?)
            // PV = -1000 + (-190.48) + 453.51 + 691.01 + 822.70
            // PV = 776.74 (approximately)
            result.NetPresentValue.Amount.Should().BeApproximately(776.74m, 0.10m);
        }

        [TestMethod]
        public void Calculate_WithHighDiscountRate_ReturnsNegativeNPV()
        {
            // Arrange
            var cashFlows = new List<CashFlow>
            {
                new CashFlow(0, new Money(-1000m, "USD")),
                new CashFlow(1, new Money(400m, "USD")),
                new CashFlow(2, new Money(400m, "USD")),
                new CashFlow(3, new Money(400m, "USD"))
            };
            var discountRate = new DiscountRate(0.5m); // 50%

            // Act
            var result = _service?.Calculate(cashFlows, discountRate);

            // Assert
            result?.NetPresentValue.Amount.Should().BeLessThan(0);
        }

        #endregion

        #region CalculateRange Method Tests

        [TestMethod]
        public void CalculateRange_WithValidInputs_ReturnsCorrectNumberOfResults()
        {
            // Arrange
            var cashFlows = new List<CashFlow>
            {
                new CashFlow(0, new Money(-1000m, "USD")),
                new CashFlow(1, new Money(500m, "USD")),
                new CashFlow(2, new Money(500m, "USD")),
                new CashFlow(3, new Money(500m, "USD"))
            };
            var lowerBound = new DiscountRate(0.01m); // 1%
            var upperBound = new DiscountRate(0.15m); // 15%
            var increment = 0.25m; // 0.25%

            // Act
            var results = _service?.CalculateRange(cashFlows, lowerBound, upperBound, increment).ToList();

            // Assert
            results.Should().HaveCount(57); // From 1% to 15% with 0.25% increment
            results.First().DiscountRate.Value.Should().Be(0.01m);
            results.Last().DiscountRate.Value.Should().BeApproximately(0.15m, 0.0001m);
        }

        [TestMethod]
        public void CalculateRange_WithEqualBounds_ReturnsSingleResult()
        {
            // Arrange
            var cashFlows = new List<CashFlow>
            {
                new CashFlow(0, new Money(-1000m, "USD")),
                new CashFlow(1, new Money(1100m, "USD"))
            };
            var bound = new DiscountRate(0.05m);
            var increment = 1m;

            // Act
            var results = _service?.CalculateRange(cashFlows, bound, bound, increment).ToList();

            // Assert
            results.Should().HaveCount(1);
            results[0].DiscountRate.Value.Should().Be(0.05m);
        }

        [TestMethod]
        public void CalculateRange_WithNullLowerBound_ThrowsArgumentNullException()
        {
            // Arrange
            var cashFlows = new List<CashFlow> { new CashFlow(0, new Money(100m, "USD")) };
            DiscountRate lowerBound = null;
            var upperBound = new DiscountRate(0.1m);

            // Act
            Action act = () => _service?.CalculateRange(cashFlows, lowerBound, upperBound, 1m);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*Bounds cannot be null*");
        }

        [TestMethod]
        public void CalculateRange_WithNullUpperBound_ThrowsArgumentNullException()
        {
            // Arrange
            var cashFlows = new List<CashFlow> { new CashFlow(0, new Money(100m, "USD")) };
            var lowerBound = new DiscountRate(0.01m);
            DiscountRate upperBound = null;

            // Act
            Action act = () => _service?.CalculateRange(cashFlows, lowerBound, upperBound, 1m);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*Bounds cannot be null*");
        }

        [TestMethod]
        public void CalculateRange_WithLowerBoundGreaterThanUpperBound_ThrowsArgumentException()
        {
            // Arrange
            var cashFlows = new List<CashFlow> { new CashFlow(0, new Money(100m, "USD")) };
            var lowerBound = new DiscountRate(0.15m);
            var upperBound = new DiscountRate(0.01m);

            // Act
            Action act = () => _service?.CalculateRange(cashFlows, lowerBound, upperBound, 1m);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Lower bound must be less than or equal to upper bound");
        }

        [TestMethod]
        public void CalculateRange_WithZeroIncrement_ThrowsArgumentException()
        {
            // Arrange
            var cashFlows = new List<CashFlow> { new CashFlow(0, new Money(100m, "USD")) };
            var lowerBound = new DiscountRate(0.01m);
            var upperBound = new DiscountRate(0.1m);

            // Act
            Action act = () => _service?.CalculateRange(cashFlows, lowerBound, upperBound, 0m);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Increment must be positive*")
                .WithParameterName("increment");
        }

        [TestMethod]
        public void CalculateRange_WithNegativeIncrement_ThrowsArgumentException()
        {
            // Arrange
            var cashFlows = new List<CashFlow> { new CashFlow(0, new Money(100m, "USD")) };
            var lowerBound = new DiscountRate(0.01m);
            var upperBound = new DiscountRate(0.1m);

            // Act
            Action act = () => _service?.CalculateRange(cashFlows, lowerBound, upperBound, -1m);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Increment must be positive*")
                .WithParameterName("increment");
        }

        [TestMethod]
        public void CalculateRange_WithNullCashFlows_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<CashFlow> cashFlows = null;
            var lowerBound = new DiscountRate(0.01m);
            var upperBound = new DiscountRate(0.1m);

            // Act
            Action act = () => _service?.CalculateRange(cashFlows, lowerBound, upperBound, 1m);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("cashFlows");
        }

        [TestMethod]
        public void CalculateRange_VerifyIncrementCorrectness()
        {
            // Arrange
            var cashFlows = new List<CashFlow>
            {
                new CashFlow(0, new Money(-1000m, "USD")),
                new CashFlow(1, new Money(1200m, "USD"))
            };
            var lowerBound = new DiscountRate(0.05m); // 5%
            var upperBound = new DiscountRate(0.10m); // 10%
            var increment = 1m; // 1%

            // Act
            var results = _service?.CalculateRange(cashFlows, lowerBound, upperBound, increment).ToList();

            // Assert
            results.Should().HaveCount(6); // 5%, 6%, 7%, 8%, 9%, 10%

            var expectedRates = new[] { 0.05m, 0.06m, 0.07m, 0.08m, 0.09m, 0.10m };
            for (int i = 0; i < results.Count; i++)
            {
                results[i].DiscountRate.Value.Should().BeApproximately(expectedRates[i], 0.0001m);
            }
        }

        [TestMethod]
        public void CalculateRange_ResultsAreInAscendingOrder()
        {
            // Arrange
            var cashFlows = new List<CashFlow>
            {
                new CashFlow(0, new Money(-1000m, "USD")),
                new CashFlow(1, new Money(600m, "USD")),
                new CashFlow(2, new Money(600m, "USD"))
            };
            var lowerBound = new DiscountRate(0.01m);
            var upperBound = new DiscountRate(0.05m);
            var increment = 0.5m; // 0.5%

            // Act
            var results = _service?.CalculateRange(cashFlows, lowerBound, upperBound, increment).ToList();

            // Assert
            for (int i = 1; i < results?.Count; i++)
            {
                results[i].DiscountRate.Value.Should().BeGreaterThan(results[i - 1].DiscountRate.Value);
            }
        }

        [TestMethod]
        [DataRow(1.0, 15.0, 0.25, 57)]
        [DataRow(5.0, 10.0, 1.0, 6)]
        [DataRow(0.0, 100.0, 10.0, 11)]
        [DataRow(10.0, 10.0, 1.0, 1)]
        public void CalculateRange_VariousRanges_ReturnsExpectedCount(
            double lowerPercent, double upperPercent, double incrementPercent, int expectedCount)
        {
            // Arrange
            var cashFlows = new List<CashFlow>
            {
                new CashFlow(0, new Money(-1000m, "USD")),
                new CashFlow(1, new Money(1500m, "USD"))
            };
            var lowerBound = new DiscountRate((decimal)(lowerPercent / 100));
            var upperBound = new DiscountRate((decimal)(upperPercent / 100));
            var increment = (decimal)incrementPercent;

            // Act
            var results = _service?.CalculateRange(cashFlows, lowerBound, upperBound, increment).ToList();

            // Assert
            results.Should().HaveCount(expectedCount);
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void CalculateRange_IntegrationTest_VerifyNPVProgression()
        {
            // Arrange
            var cashFlows = new List<CashFlow>
            {
                new CashFlow(0, new Money(-1000m, "USD")),
                new CashFlow(1, new Money(400m, "USD")),
                new CashFlow(2, new Money(400m, "USD")),
                new CashFlow(3, new Money(400m, "USD"))
            };
            var lowerBound = new DiscountRate(0.05m);
            var upperBound = new DiscountRate(0.15m);
            var increment = 5m; // 5%

            // Act
            var results = _service?.CalculateRange(cashFlows, lowerBound, upperBound, increment).ToList();

            // Assert
            results.Should().HaveCount(3);

            // NPV should decrease as discount rate increases
            results[0].NetPresentValue.Amount.Should().BeGreaterThan(results[1].NetPresentValue.Amount);
            results[1].NetPresentValue.Amount.Should().BeGreaterThan(results[2].NetPresentValue.Amount);

            // All should have same currency
            results.Should().AllSatisfy(r => r.NetPresentValue.Currency.Should().Be("USD"));
        }

        #endregion
    }
}