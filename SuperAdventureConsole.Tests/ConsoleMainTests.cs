using Engine;
using SuperAdventureConsole;
using FluentAssertions;
using Ploeh.AutoFixture;
using Xunit;
using Ploeh.AutoFixture.Xunit2;

namespace SuperAdventureConsole_Tests
{
    public class LoadGame
    {
        [Fact]
        public void LoadGameDataLoadsXmlIfPresent()
        {
            var sut = Player.CreateDefaultPlayer();
            sut.Gold = -1;

        }

        [Theory]
        [InlineAutoData]
        [InlineAutoData(2)]
        [InlineAutoData(3)]
        public void test_InlineData(int a)
        {
            //Fixture fixture = 
            var sut = 2;

            a.Should().Be(sut, "the incoming parameter 'a' needs to be 2");
        }
    }
}
