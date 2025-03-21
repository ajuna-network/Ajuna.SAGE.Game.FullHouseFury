namespace Ajuna.SAGE.Game.FullHouseFury
{
    public enum FullHouseFuryAction : byte
    {
        None = 0,
        Start = 1,
        Play = 2,
        Preparation = 3,
        Battle = 4,
        Discard = 5,
        Score = 6,
        Shop = 7,
        // *** DO NOT PASS 15 INDEX ***
    }

    public enum FullHouseFuryRuleType : byte
    {
        None = 0,
        AssetCount = 1,
        AssetTypeIs = 2,
        IsOwnerOf = 3,
        SameExist = 4,
        SameNotExist = 5,
        AssetTypesAt = 6,
        BalanceOf = 7,
        IsOwnerOfAll = 8,
        HasCooldownOf = 9,
        // *** DO NOT PASS 15 INDEX ***
    }

    public enum FullHouseFuryRuleOp : byte
    {
        None = 0,
        EQ = 1,
        GT = 2,
        LT = 3,
        GE = 4,
        LE = 5,
        NE = 6,
        Index = 7,
        MatchType = 8,
        Composite = 9,
        // *** DO NOT PASS 15 INDEX ***
    }

    public enum AssetType
    {
        None = 0,
        Tower = 1,
        Deck = 2,
        Game = 3,
        // *** DO NOT PASS 15 INDEX ***
    }

    public enum AssetSubType
    {
        None = 0,
        // *** DO NOT PASS 15 INDEX ***
    }

    public enum GameState
    {
        None = 0,
        Running = 1,
        Finished = 2,
    }

    public enum GameEvent
    {
        None = 0,
        OnLevelStart,
        OnRoundStart,
        OnAttack,
        OnDraw,
        OnDiscard
    }

    public enum LevelState
    {
        None = 0,
        Preparation = 1,
        Battle = 2,
        Score = 3,
    }

    public enum Suit
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades
    }

    public enum Rank
    {
        Ace = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }

    public enum PokerHand
    {
        None = 0,
        HighCard = 1,
        Pair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush,
        RoyalFlush
    }

    /// <summary>
    /// Features that can be upgraded in the game.
    /// </summary>
    public enum FeatureType
    {
        None = 0,
        RarityLevel = 1,
        PokerHandLevel = 2,
    }

    /// <summary>
    /// Defines different types of bonuses that can be applied in the game.
    /// </summary>
    public enum BonusType
    {
        None = 0,
        DeckRefill = 1,
        ExtraEndurance = 2,
        HeartHeal = 3,
        DamageBoost = 4,
        ExtraCardDraw = 5,
        FaceCardBonus = 6,
        SuitDiversityBonus = 7,
        LuckyDraw = 8,
        CriticalStrikeChance = 9,
        RapidRecovery = 10,
        ShieldOfValor = 11,
        MysticInsight = 12,
        ArcaneSurge = 13,
        RighteousFury = 14,
        BlessedAura = 15,
        FortunesFavor = 16,
        NimbleFingers = 17,
        EagleEye = 18,
        UnyieldingSpirit = 19,
        DivineIntervention = 20,
        ZealousCharge = 21,
        RelentlessAssault = 22,
        VitalStrike = 23,
        PurityOfHeart = 24,
        CelestialGuidance = 25,
        SwiftReflexes = 26,
        InspiringPresence = 27,
        Serendipity = 28,
        ArcaneWisdom = 29,
        MajesticRoar = 30,
        FortuitousWinds = 31,
        StalwartResolve = 32
    }

    /// <summary>
    /// Defines different types of maluses (negative effects) that can be applied in the game.
    /// </summary>
    public enum MalusType
    {
        None = 0,
        HalvedDamage = 1,
        SpadeHeal = 2,
        ReducedEndurance = 3,
        IncreasedFatigueRate = 4,
        LowerCardValue = 5,
        NumberCardPenalty = 6,
        UniformSuitPenalty = 7,
        Misfortune = 8,
        SluggishRecovery = 9,
        CursedDraw = 10,
        WeakStrike = 11,
        UnluckyTiming = 12,
        BlightedAura = 13,
        CrumblingDeck = 14,
        DiminishedInsight = 15,
        VulnerableState = 16,
        RecklessPlay = 17,
        WeakenedSpirit = 18,
        GrimFate = 19,
        SlipperyFingers = 20,
        BloodCurse = 21,
        Despair = 22,
        FracturedWill = 23,
        EnervatingPresence = 24,
        MisalignedFocus = 25,
        HeavyBurden = 26,
        StumblingStep = 27,
        DimmedVision = 28,
        FadingResolve = 29,
        ToxicMiasma = 30,
        CursedFate = 31,
        SourLuck = 32
    }

}