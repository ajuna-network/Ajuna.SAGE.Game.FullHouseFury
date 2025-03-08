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
        public readonly FullHouseFuryRuleOp RuleOpEnum => (FullHouseFuryRuleOp)RuleOp;

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
            : this(type, FullHouseFuryRuleOp.None, Array.Empty<byte>())
        {
        }

        public FullHouseFuryRule(FullHouseFuryRuleType type, FullHouseFuryRuleOp operation, byte i0)
            : this(type, operation, new byte[] { i0 })
        {
        }

        public FullHouseFuryRule(FullHouseFuryRuleType type, FullHouseFuryRuleOp operation, byte i0, byte i1)
            : this(type, operation, new byte[] { i0, i1 })
        {
        }

        public FullHouseFuryRule(FullHouseFuryRuleType type, FullHouseFuryRuleOp operation, byte i0, byte i1, byte i2)
            : this(type, operation, new byte[] { i0, i1, i2 })
        {
        }

        public FullHouseFuryRule(FullHouseFuryRuleType type, FullHouseFuryRuleOp operation, byte i0, byte i1, byte i2, byte i3)
            : this(type, operation, new byte[] { i0, i1, i2, i3 })
        {
        }
    }
}