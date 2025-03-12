using NUnit.Framework;
using Ajuna.SAGE.Game.FullHouseFury.Model;
using Ajuna.SAGE.Core.Model;
using System;
using System.Linq;

namespace Ajuna.SAGE.Game.FullHouseFury.Test.Model
{
    [TestFixture]
    public class TowerAssetTests
    {
        private TowerAsset towerAsset;

        [SetUp]
        public void Setup()
        {
            // TowerAsset constructor initializes Data with a 32-byte array.
            towerAsset = new TowerAsset(1, 1);
        }

        #region Single Boons Tests

        [Test]
        public void SingleBoons_Default_IsZero()
        {
            towerAsset.SingleBoons = 0;
            Assert.That(towerAsset.SingleBoons, Is.EqualTo(0), "SingleBoons should be 0 when not set.");
        }

        [Test]
        public void SetSingleBoon_ValidValue_Works()
        {
            // For boons at index 0-31, valid values are 0 or 1.
            towerAsset.SingleBoons = 0;
            towerAsset.SetBoon(5, 1);
            uint expected = (uint)(1 << 5);
            Assert.That(towerAsset.SingleBoons, Is.EqualTo(expected), "Bit 5 should be set in SingleBoons.");
        }

        [Test]
        public void SetSingleBoon_InvalidValue_ThrowsException()
        {
            // For an index < 32, value > 1 is invalid.
            Assert.Throws<ArgumentOutOfRangeException>(() => towerAsset.SetBoon(10, 2));
        }

        #endregion

        #region Multi Boons Tests

        [Test]
        public void SetMultiBoon_ValidValues_Works()
        {
            // For boons at index 32-47, valid values are 0, 1, 2, or 3.
            towerAsset.MultiBoons = 0;
            towerAsset.SetBoon(33, 3); // 33 - 32 = 1 offset in MultiBoons.
            uint multi = towerAsset.MultiBoons;
            uint mask = (uint)(3 << 1); // At offset 1.
            uint expected = (uint)(3 << 1);
            Assert.That(multi & mask, Is.EqualTo(expected), "MultiBoon at index 33 should be set to 3.");
        }

        [Test]
        public void SetMultiBoon_InvalidValue_ThrowsException()
        {
            // For an index in 32-47, a value > 3 is invalid.
            Assert.Throws<ArgumentOutOfRangeException>(() => towerAsset.SetBoon(40, 4));
        }

        #endregion

        #region GetAllBoons Tests

        [Test]
        public void GetAllBoons_ReturnsExpectedArray()
        {
            // Reset boons.
            towerAsset.SingleBoons = 0;
            towerAsset.MultiBoons = 0;
            // Set a single boon at index 2.
            towerAsset.SetBoon(2, 1);
            // Set a multi boon at index 33 (offset 33 - 32 = 1) to 3.
            towerAsset.SetBoon(33, 3);

            byte[] boons = towerAsset.GetAllBoons();
            // For single boons, index 2 should be 1.
            Assert.That(boons[2], Is.EqualTo(1), "Single boon at index 2 should be 1.");
            // For multi boons, index 33 corresponds to offset 1 in MultiBoons; so expected value is 3.
            Assert.That(boons[33], Is.EqualTo(3), "Multi boon at index 33 should be 3.");
        }

        #endregion

        #region Single Banes Tests

        [Test]
        public void SingleBanes_Default_IsZero()
        {
            towerAsset.SingleBanes = 0;
            Assert.That(towerAsset.SingleBanes, Is.EqualTo(0), "SingleBanes should be 0 when not set.");
        }

        [Test]
        public void SetSingleBane_ValidValue_Works()
        {
            towerAsset.SingleBanes = 0;
            towerAsset.SetBanes(7, 1); // For banes, index 7 (0-31) should be 0 or 1.
            uint expected = (uint)(1 << 7);
            Assert.That(towerAsset.SingleBanes, Is.EqualTo(expected), "Bit 7 should be set in SingleBanes.");
        }

        [Test]
        public void SetSingleBane_InvalidValue_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => towerAsset.SetBanes(15, 2));
        }

        #endregion

        #region Multi Banes Tests

        [Test]
        public void SetMultiBane_ValidValues_Works()
        {
            towerAsset.MultiBanes = 0;
            towerAsset.SetBanes(34, 2); // For banes, index 34 => offset = 34 - 32 = 2.
            uint multi = towerAsset.MultiBanes;
            uint mask = (uint)(3 << 2);
            uint expected = (uint)(2 << 2);
            Assert.That(multi & mask, Is.EqualTo(expected), "MultiBane at index 34 should be set to 2.");
        }

        [Test]
        public void SetMultiBane_InvalidValue_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => towerAsset.SetBanes(45, 4));
        }

        #endregion

        #region GetAllBanes Tests

        [Test]
        public void GetAllBanes_ReturnsExpectedArray()
        {
            towerAsset.SingleBanes = 0;
            towerAsset.MultiBanes = 0;
            towerAsset.SetBanes(1, 1);   // Set single bane at index 1.
            towerAsset.SetBanes(34, 2);  // Set multi bane at index 34 (offset = 2).

            byte[] banes = towerAsset.GetAllBanes();
            // For single banes, index 1 should be 1.
            Assert.That(banes[1], Is.EqualTo(1), "Single bane at index 1 should be 1.");
            // For multi banes, index 34 should yield value 2.
            Assert.That(banes[34], Is.EqualTo(2), "Multi bane at index 34 should be 2.");
        }

        #endregion
    }
}
