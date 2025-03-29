using Ajuna.SAGE.Game.FullHouseFury.Effects;
using Ajuna.SAGE.Game.FullHouseFury.Model;

namespace Ajuna.SAGE.Game.FullHouseFury.Test.Model;

public class FxBoonsTest
{
    private GameAsset _game;
    private DeckAsset _deck;
    private TowerAsset _towr;

    [SetUp]
    public void Setup()
    {
        _game = new GameAsset(1, 1);
        _deck = new DeckAsset(1, 1);
        _towr = new TowerAsset(1, 1);
    }

    [Test]
    public void FxSuitHeal_Test()
    {
        var heart6 = new Card(Suit.Hearts, Rank.Six);
        var spade6 = new Card(Suit.Spades, Rank.Six);

        var heart7 = new Card(Suit.Hearts, Rank.Seven);
        var spade7 = new Card(Suit.Spades, Rank.Seven);

        _game.PlayerDamage = 24;
        Assert.That(_game.PlayerDamage, Is.EqualTo(24));

        var fxSuitHeal = new FxSuitHeal(Suit.Hearts);
        fxSuitHeal.Apply(GameEvent.OnAttack, _game, _deck, _towr, 1, new AttackContext(PokerHand.Pair, 10, new byte[] { heart6.Index, spade6.Index }));

        Assert.That(_game.PlayerDamage,Is.EqualTo(18));

        fxSuitHeal.Apply(GameEvent.OnAttack, _game, _deck, _towr, 1, new AttackContext(PokerHand.TwoPair, 10, new byte[] { heart6.Index, spade6.Index, heart7.Index, spade7.Index }));

        Assert.That(_game.PlayerDamage, Is.EqualTo(5));

        fxSuitHeal.Apply(GameEvent.OnAttack, _game, _deck, _towr, 1, new AttackContext(PokerHand.HighCard, 10, new byte[] { spade7.Index, heart6.Index }));

        Assert.That(_game.PlayerDamage, Is.EqualTo(0));

    }
}
