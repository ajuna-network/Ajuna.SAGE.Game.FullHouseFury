using Ajuna.SAGE.Game.FullHouseFury.Effects;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using System.Xml.Linq;
using static Ajuna.SAGE.Game.FullHouseFury.Effects.FxFaceCardBonus;

namespace Ajuna.SAGE.Game.FullHouseFury.Test.Model;

public class FxBoonsTest
{
    public Card HE_6 = new Card(Suit.Hearts, Rank.Six);
    public Card SP_6 = new Card(Suit.Spades, Rank.Six);

    public Card HE_7 = new Card(Suit.Hearts, Rank.Seven);
    public Card SP_7 = new Card(Suit.Spades, Rank.Seven);

    public Card HE_J = new Card(Suit.Hearts, Rank.Jack);
    public Card SP_J = new Card(Suit.Spades, Rank.Jack);

    private GameAsset _game;
    private DeckAsset _deck;
    private TowerAsset _towr;

    [SetUp]
    public void Setup()
    {
        _game = new GameAsset(1, 1);
        _deck = new DeckAsset(1, 1);
        _towr = new TowerAsset(1, 1);

        _game.New();
        _deck.New();
        _towr.New();
    }

    [Test]
    public void FxSuitHeal_Test()
    {
        var fx = new FxSuitHeal(Suit.Hearts);

        _game.PlayerDamage = 24;
        Assert.That(_game.PlayerDamage, Is.EqualTo(24));

        fx.Apply(GameEvent.OnAttack, _game, _deck, _towr, 1, new AttackContext(PokerHand.Pair, 10, [HE_6.Index, SP_6.Index]));

        Assert.That(_game.PlayerDamage,Is.EqualTo(18));

        fx.Apply(GameEvent.OnAttack, _game, _deck, _towr, 1, new AttackContext(PokerHand.TwoPair, 10, [HE_6.Index, SP_6.Index, HE_7.Index, SP_7.Index]));

        Assert.That(_game.PlayerDamage, Is.EqualTo(5));

        fx.Apply(GameEvent.OnAttack, _game, _deck, _towr, 1, new AttackContext(PokerHand.HighCard, 10, [SP_7.Index, HE_6.Index]));

        Assert.That(_game.PlayerDamage, Is.EqualTo(0));
    }

    [Test]
    public void FxExtraCardDraw_Test()
    {
        var fx = new FxExtraCardDraw();

        Assert.That(_game.HandSize, Is.EqualTo(7));

        // Add 1 card to hand

        fx.Add(_game, _deck, _towr, 1, new ModifyContext(0, 1));
        Assert.That(_game.HandSize, Is.EqualTo(8));

        // Can't progress past MAX_HAND_SIZE of 8

        fx.Add(_game, _deck, _towr, 1, new ModifyContext(0, 1));
        Assert.That(_game.HandSize, Is.EqualTo(8));

        // Remove 1 card to hand

        fx.Remove(_game, _deck, _towr, 0, new ModifyContext(1, 0));
        Assert.That(_game.HandSize, Is.EqualTo(7));

    }

    [Test]
    public void FxFaceCardBonus_Test()
    {
        var fx = new FxFaceCardBonus();

        _game.AttackScore = 10;

        Assert.That(_game.AttackScore, Is.EqualTo(10));

        fx.Apply(GameEvent.OnAttack, _game, _deck, _towr, 1, new AttackContext(PokerHand.Pair, _game.AttackScore, [HE_J.Index, SP_J.Index]));

        Assert.That(_game.AttackScore, Is.EqualTo(32));

    }

    [Test]
    public void FxEnduranceUp_Test()
    {
        var fx = new FxEnduranceUp();

        Assert.That(_game.MaxPlayerEndurance, Is.EqualTo(10));
        Assert.That(_game.PlayerEndurance, Is.EqualTo(10));

        // Add 1 endurance

        fx.Add(_game, _deck, _towr, 1, new ModifyContext(0, 1));
        Assert.That(_game.MaxPlayerEndurance, Is.EqualTo(11));
        Assert.That(_game.PlayerEndurance, Is.EqualTo(11));

        // Remove 1 endurance

        fx.Remove(_game, _deck, _towr, 0, new ModifyContext(1, 0));
        Assert.That(_game.MaxPlayerEndurance, Is.EqualTo(10));
        Assert.That(_game.PlayerEndurance, Is.EqualTo(10));

    }

    [Test]
    public void FxDeckRefill_Test()
    {
        var fx = new FxDeckRefill();

        Assert.That(_deck.DeckRefill, Is.EqualTo(0));

        // Add 1 deck refill

        fx.Add(_game, _deck, _towr, 1, new ModifyContext(0, 1));
        Assert.That(_deck.DeckRefill, Is.EqualTo(1));

        // Remove 1 deck refill

        fx.Remove(_game, _deck, _towr, 0, new ModifyContext(1, 0));
        Assert.That(_deck.DeckRefill, Is.EqualTo(0));

    }
}
