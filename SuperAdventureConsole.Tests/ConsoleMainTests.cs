using Engine;
using SuperAdventureConsole;
using Xunit;

namespace SuperAdventureConsole_Tests
{
    public class ConsoleMainTests
    {
        [Fact]
        public void LoadGameDataLoadsXmlIfPresent()
        {
            var sut = Player.CreateDefaultPlayer();
            sut.Gold = -1;

        }
    }
}
