using Engine;
using FluentAssertions;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit2;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.AutoMoq.Extensions;
using Xunit;

namespace Engine_Tests
{
    public class Player_Test
    {
        [Fact]
        public void DefaultPlayerStatsAreCorrect()
        {
            var sut = Player.CreateDefaultPlayer();
            //var fixture = new Fixture();

            sut.CurrentHitPoints.Should().Be(10, "default Player.CurrentHitPoints is 10");
            sut.MaximumHitPoints.Should().Be(10, "default Player.MaximumHitPoints is 10");
            sut.Gold.Should().Be(20,"default Player.Gold is 20");
            sut.ExperiencePoints.Should().Be(0,"default Player.ExperiencePoints are 0");
        }

        [Fact]
        public void DefaultPlayerWeaponIsRustySword()
        {
            var sut = Player.CreateDefaultPlayer();
            //var fixture = new Fixture();

            sut.Weapons.Count.Should().BeGreaterOrEqualTo(1);
            sut.Weapons[0].ID.Should().Be(World.ITEM_ID_RUSTY_SWORD,
                "The first Weapon in the list for a Default Player must be a Rusty Sword");
        }

        [Fact]
        public void DefaultPlayerInventoryStartsWithOneItem()
        {
            var sut = Player.CreateDefaultPlayer();

            sut.Inventory.Count.Should().Be(1, "Inventory count should be 1 at startup with only item being a Rusty Sword");
        }

        [Fact]
        public void AddExperiencePoints_IncreasesLevel()
        {
            var fix = new Fixture().Customize(new AutoMoqCustomization());
            var sut = fix.Create<Player>();

            var levelstart = sut.Level;
            sut.AddExperiencePoints(100);
            sut.Level.Should().Be(levelstart + 1, "we added 100 xp which should raise the level by 1");
        }
    }
}
