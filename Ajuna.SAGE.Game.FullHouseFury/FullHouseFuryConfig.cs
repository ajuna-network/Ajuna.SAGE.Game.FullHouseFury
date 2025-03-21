using Ajuna.SAGE.Core.Model;

namespace Ajuna.SAGE.Game.FullHouseFury
{
    public class FullHouseFuryConfig
    {
        public static FullHouseFuryIdentifier Start(out FullHouseFuryRule[] rules, out ITransitioFee? fee)
        {
            byte gameType = FullHouseFuryUtil.MatchType(AssetType.Game);
            byte deckType = FullHouseFuryUtil.MatchType(AssetType.Deck);
            byte towrType = FullHouseFuryUtil.MatchType(AssetType.Tower);

            rules = new FullHouseFuryRule[] {
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetCount, FullHouseFuryRuleOp.EQ, 0u),
                new FullHouseFuryRule(FullHouseFuryRuleType.SameNotExist, FullHouseFuryRuleOp.MatchType, gameType),
                new FullHouseFuryRule(FullHouseFuryRuleType.SameNotExist, FullHouseFuryRuleOp.MatchType, deckType),
                new FullHouseFuryRule(FullHouseFuryRuleType.SameNotExist, FullHouseFuryRuleOp.MatchType, towrType),
            };

            fee = default;

            return FullHouseFuryIdentifier.Create(FullHouseFuryAction.Start);
        }

        public static FullHouseFuryIdentifier Play(out FullHouseFuryRule[] rules, out ITransitioFee? fee)
        {
            byte gameType = FullHouseFuryUtil.MatchType(AssetType.Game);
            byte deckType = FullHouseFuryUtil.MatchType(AssetType.Deck);
            byte towrType = FullHouseFuryUtil.MatchType(AssetType.Tower);

            rules = new FullHouseFuryRule[] {
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetCount, FullHouseFuryRuleOp.EQ, 3u),
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetTypesAt, FullHouseFuryRuleOp.Composite, gameType, deckType, towrType),
                // TODO: verify gamestate is running in rules
            };

            fee = default;

            return FullHouseFuryIdentifier.Create(FullHouseFuryAction.Play);
        }

        public static FullHouseFuryIdentifier Preparation(out FullHouseFuryRule[] rules, out ITransitioFee? fee)
        {
            byte gameType = FullHouseFuryUtil.MatchType(AssetType.Game);
            byte deckType = FullHouseFuryUtil.MatchType(AssetType.Deck);
            byte towrType = FullHouseFuryUtil.MatchType(AssetType.Tower);

            rules = new FullHouseFuryRule[] {
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetCount, FullHouseFuryRuleOp.EQ, 3u),
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetTypesAt, FullHouseFuryRuleOp.Composite, gameType, deckType, towrType),
                // TODO: verify gamestate is running in rules
            };

            fee = default;

            return FullHouseFuryIdentifier.Create(FullHouseFuryAction.Preparation);
        }

        public static FullHouseFuryIdentifier Battle(out FullHouseFuryRule[] rules, out ITransitioFee? fee)
        {
            byte gameType = FullHouseFuryUtil.MatchType(AssetType.Game);
            byte deckType = FullHouseFuryUtil.MatchType(AssetType.Deck);
            byte towrType = FullHouseFuryUtil.MatchType(AssetType.Tower);

            rules = new FullHouseFuryRule[] {
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetCount, FullHouseFuryRuleOp.EQ, 3u),
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetTypesAt, FullHouseFuryRuleOp.Composite, gameType, deckType, towrType),
                // TODO: verify gamestate is running in rules
            };

            fee = default;

            return FullHouseFuryIdentifier.Create(FullHouseFuryAction.Battle);
        }

        public static FullHouseFuryIdentifier Discard(out FullHouseFuryRule[] rules, out ITransitioFee? fee)
        {
            byte gameType = FullHouseFuryUtil.MatchType(AssetType.Game);
            byte deckType = FullHouseFuryUtil.MatchType(AssetType.Deck);
            byte towrType = FullHouseFuryUtil.MatchType(AssetType.Tower);

            rules = new FullHouseFuryRule[] {
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetCount, FullHouseFuryRuleOp.EQ, 3u),
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetTypesAt, FullHouseFuryRuleOp.Composite, gameType, deckType, towrType),
                // TODO: verify gamestate is running in rules
            };

            fee = default;

            return FullHouseFuryIdentifier.Create(FullHouseFuryAction.Discard);
        }

        public static FullHouseFuryIdentifier Score(out FullHouseFuryRule[] rules, out ITransitioFee? fee)
        {
            byte gameType = FullHouseFuryUtil.MatchType(AssetType.Game);
            byte deckType = FullHouseFuryUtil.MatchType(AssetType.Deck);
            byte towrType = FullHouseFuryUtil.MatchType(AssetType.Tower);

            rules = new FullHouseFuryRule[] {
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetCount, FullHouseFuryRuleOp.EQ, 3u),
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetTypesAt, FullHouseFuryRuleOp.Composite, gameType, deckType, towrType),
                // TODO: verify gamestate is running in rules
            };

            fee = default;

            return FullHouseFuryIdentifier.Create(FullHouseFuryAction.Score);
        }

        public static FullHouseFuryIdentifier Shop(out FullHouseFuryRule[] rules, out ITransitioFee? fee)
        {
            byte gameType = FullHouseFuryUtil.MatchType(AssetType.Game);
            byte deckType = FullHouseFuryUtil.MatchType(AssetType.Deck);
            byte towrType = FullHouseFuryUtil.MatchType(AssetType.Tower);

            rules = new FullHouseFuryRule[] {
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetCount, FullHouseFuryRuleOp.EQ, 3u),
                new FullHouseFuryRule(FullHouseFuryRuleType.AssetTypesAt, FullHouseFuryRuleOp.Composite, gameType, deckType, towrType),
                // TODO: verify gamestate is running in rules
            };

            fee = default;

            return FullHouseFuryIdentifier.Create(FullHouseFuryAction.Shop);
        }
    }
}