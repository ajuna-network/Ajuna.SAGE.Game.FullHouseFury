using Ajuna.SAGE.Core.Model;
using System;

namespace Ajuna.SAGE.Game.FullHouseFury
{
    public struct FullHouseFuryIdentifier : ITransitionIdentifier
    {
        public byte TransitionType { get; set; }
        public byte TransitionSubType { get; set; }

        public FullHouseFuryIdentifier(byte transitionType, byte transitionSubType)
        {
            TransitionType = transitionType;
            TransitionSubType = transitionSubType;
        }

        public FullHouseFuryIdentifier(byte transitionType) : this(transitionType, 0)
        {
        }

        public static FullHouseFuryIdentifier Start()
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Start);

        public static FullHouseFuryIdentifier Play()
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Play);

        public static FullHouseFuryIdentifier Preparation()
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Preparation);

        public static FullHouseFuryIdentifier Battle()
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Battle);

        public static FullHouseFuryIdentifier Discard()
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Discard);

        public static FullHouseFuryIdentifier Score()
            => new FullHouseFuryIdentifier((byte)FullHouseFuryAction.Score);

    }
}