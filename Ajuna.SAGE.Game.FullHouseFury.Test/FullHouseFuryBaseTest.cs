using Ajuna.SAGE.Core.Model;
using Ajuna.SAGE.Game.FullHouseFury;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Core.HeroJam.Test
{
    public class FullHouseFuryBaseTest
    {
        public IBlockchainInfoProvider BlockchainInfoProvider { get; private set; }
        public Engine<FullHouseFuryIdentifier, FullHouseFuryRule> Engine { get; private set; }

        public FullHouseFuryBaseTest()
        {
            BlockchainInfoProvider = new BlockchainInfoProvider(1234);
            Engine = FullHouseFuryGame.Create(BlockchainInfoProvider);
        }

        public void Reset()
        {
            BlockchainInfoProvider = new BlockchainInfoProvider(1234);
            Engine = FullHouseFuryGame.Create(BlockchainInfoProvider);
        }

        public T GetAsset<T>(IAccount user, AssetType type) where T : BaseAsset
        {
            BaseAsset? result = Engine.AssetManager
                .AssetOf(user)
                .Select(p => (BaseAsset)p)
                .Where(p => p.AssetType == type)
                .FirstOrDefault();
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<T>());
            var typedResult = result as T;
            Assert.That(typedResult, Is.Not.Null);
            return typedResult;
        }
    }
}
