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

    public enum  GameState
    {
        None = 0,
        Running = 1,
        Finished = 2,
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
}