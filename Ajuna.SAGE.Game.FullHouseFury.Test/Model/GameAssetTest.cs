using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Game.FullHouseFury.Test.Model
{
    [TestFixture]
    public class GameAssetTest
    {
        private GameAsset gameAsset;

        [SetUp]
        public void Setup()
        {
            gameAsset = new GameAsset(1, 1);

        }

    }
}