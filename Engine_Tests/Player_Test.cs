using Engine;
using FluentAssertions;
using Ploeh.AutoFixture;
using Xunit;

namespace Engine_Tests
{
    public class Player_Test
    {
        [Fact]
        public void DefaultPlayerStatsAreCorrect()
        {
            var sut = Player.CreateDefaultPlayer();
            var fixture = new Fixture();

            sut.CurrentHitPoints.Should().Be(10, "default Player.CurrentHitPoints is 10");
            sut.MaximumHitPoints.Should().Be(10, "default Player.MaximumHitPoints is 10");
            sut.Gold.Should().Be(20,"default Player.Gold is 20");
            sut.ExperiencePoints.Should().Be(0,"default Player.ExperiencePoints are 0");
        }
    }
}
