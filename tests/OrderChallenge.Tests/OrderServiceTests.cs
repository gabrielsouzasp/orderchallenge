using OrderAccumulator.Services;

namespace OrderChallenge.Tests
{
    public class OrderServiceTests
    {
        private readonly OrderService _service = new();

        [Fact]
        public void Buy_IncreasesExposure()
        {
            Assert.True(_service.AddOrder("PETR4", 10.50m, 100m, isBuy: true));

            Assert.Equal(1_050m, _service.GetExposure("PETR4"));
        }

        [Fact]
        public void Sell_DecreasesExposure()
        {
            _service.AddOrder("PETR4", 10m, 100m, isBuy: true);

            Assert.True(_service.AddOrder("PETR4", 10m, 30m, isBuy: false));

            Assert.Equal(700m, _service.GetExposure("PETR4"));
        }

        [Fact]
        public void Sell_WithoutPosition_MakesExposureNegative()
        {
            Assert.True(_service.AddOrder("VALE3", 20m, 50m, isBuy: false));

            Assert.Equal(-1_000m, _service.GetExposure("VALE3"));
        }

        [Fact]
        public void UnknownSymbol_HasZeroExposure()
        {
            Assert.Equal(0m, _service.GetExposure("VIIA4"));
        }

        [Fact]
        public void Order_ReachingExactlyTheLimit_IsAccepted()
        {
            Assert.True(_service.AddOrder("PETR4", 1_000m, 100_000m, isBuy: true));

            Assert.Equal(100_000_000m, _service.GetExposure("PETR4"));
        }

        [Fact]
        public void Buy_ExceedingLimit_IsRejectedAndNotCounted()
        {
            _service.AddOrder("PETR4", 1_000m, 100_000m, isBuy: true);

            Assert.False(_service.AddOrder("PETR4", 0.01m, 1m, isBuy: true));

            Assert.Equal(100_000_000m, _service.GetExposure("PETR4"));
        }

        [Fact]
        public void Sell_ExceedingNegativeLimit_IsRejectedAndNotCounted()
        {
            _service.AddOrder("PETR4", 1_000m, 100_000m, isBuy: false);

            Assert.False(_service.AddOrder("PETR4", 0.01m, 1m, isBuy: false));

            Assert.Equal(-100_000_000m, _service.GetExposure("PETR4"));
        }

        [Fact]
        public void Sell_AfterLimitReached_FreesRoomForNewBuys()
        {
            _service.AddOrder("PETR4", 1_000m, 100_000m, isBuy: true);
            _service.AddOrder("PETR4", 500m, 10_000m, isBuy: false);

            Assert.True(_service.AddOrder("PETR4", 500m, 10_000m, isBuy: true));
            Assert.Equal(100_000_000m, _service.GetExposure("PETR4"));
        }

        [Fact]
        public void Sell_FromPositiveLimit_CanCrossToNegativeWithinLimit()
        {
            _service.AddOrder("PETR4", 1_000m, 100_000m, isBuy: true);

            Assert.True(_service.AddOrder("PETR4", 1_000m, 200_000m, isBuy: false));
            Assert.Equal(-100_000_000m, _service.GetExposure("PETR4"));
        }

        [Fact]
        public void Limit_IsIndependentPerSymbol()
        {
            _service.AddOrder("PETR4", 1_000m, 100_000m, isBuy: true);

            Assert.True(_service.AddOrder("VALE3", 1_000m, 100_000m, isBuy: true));
            Assert.Equal(100_000_000m, _service.GetExposure("VALE3"));
            Assert.Equal(0m, _service.GetExposure("VIIA4"));
        }
    }
}
