using Conference.Functions.Services;
using Xunit;

namespace Conference.Functions.Tests
{
    public class CapacityManagerTests
    {
        [Fact]
        public void WhenEnforceFalse_ReturnsConfirmed()
        {
            var s = CapacityManager.DecideStatus(100, 50, 10, 5, false);
            Assert.Equal("confirmed", s);
        }

        [Fact]
        public void WhenCapacityAvailable_ReturnsConfirmed()
        {
            var s = CapacityManager.DecideStatus(5, 0, 10, 5, true);
            Assert.Equal("confirmed", s);
        }

        [Fact]
        public void WhenFullButWaitlistAvailable_ReturnsWaitlisted()
        {
            var s = CapacityManager.DecideStatus(10, 0, 10, 5, true);
            Assert.Equal("waitlisted", s);
        }

        [Fact]
        public void WhenFullAndWaitlistFull_ReturnsRejected()
        {
            var s = CapacityManager.DecideStatus(10, 5, 10, 5, true);
            Assert.Equal("rejected", s);
        }

        [Fact]
        public void WhenUnlimitedCapacity_ReturnsConfirmed()
        {
            var s = CapacityManager.DecideStatus(1000, 0, 0, 0, true);
            Assert.Equal("confirmed", s);
        }
    }
}