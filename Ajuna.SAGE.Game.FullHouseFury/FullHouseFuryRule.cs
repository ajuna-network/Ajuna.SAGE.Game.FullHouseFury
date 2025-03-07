using Ajuna.SAGE.Core.Model;
using System;

namespace Ajuna.SAGE.Game.FullHouseFury
{
    public struct FullHouseFuryRule : ITransitionRule
    {
        public byte RuleType { get; private set; }
        public byte RuleOp { get; private set; }
        public byte[] RuleValue { get; private set; }

        public readonly FullHouseFuryRuleType RuleTypeEnum => (FullHouseFuryRuleType)RuleType;
        public readonly FullHouseFuryRuleOp RuleOpEnum => (FullHouseFuryRuleOp)(RuleOp >> 4);

        // Existing constructors using byte[] or uint as value:
        public FullHouseFuryRule(FullHouseFuryRuleType type, FullHouseFuryRuleOp operation, byte[] value)
        {
            RuleType = (byte)type;
            RuleOp = (byte)operation;
            RuleValue = value;
        }

        public FullHouseFuryRule(FullHouseFuryRuleType type, FullHouseFuryRuleOp operation, uint value)
            : this(type, operation, BitConverter.GetBytes(value))
        {
        }

        public FullHouseFuryRule(FullHouseFuryRuleType type, FullHouseFuryRuleOp operation)
            : this(type, operation, Array.Empty<byte>())
        {
        }

        public FullHouseFuryRule(FullHouseFuryRuleType type)
            : this(type, FullHouseFury.FullHouseFuryRuleOp.None, Array.Empty<byte>())
        {
        }
    }
}