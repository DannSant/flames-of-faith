using Game.Combat;
using Game.Combat.Elemental;
using NUnit.Framework;
using System.IO;

namespace Game.Tests
{
    /// <summary>
    /// A Ley Charged enemy strikes its closest neighbour when it is hit, not on burn or bleed ticks:
    /// a burning, charged enemy used to strike the same neighbour on every tick (6 times over a 3 s burn).
    /// </summary>
    public class LeyChargeTests
    {
        private const int MaxDepth = 3;

        [Test]
        public void AHit_Strikes()
        {
            Assert.IsTrue(DebuffLeyCharge.ShouldStrike(new DamageRequest(10f, WeaponClass.Ranged, true), MaxDepth));
        }

        [Test]
        public void ADamageOverTimeTick_DoesNotStrike()
        {
            Assert.IsFalse(DebuffLeyCharge.ShouldStrike(DamageRequest.DamageOverTime(2f, WeaponClass.Magic), MaxDepth));
        }

        [Test]
        public void ASilentForcedKill_DoesNotStrike()
        {
            // The wave end kills the enemies left silently; they must not fire bolts as they go
            var kill = new DamageRequest(9999f, WeaponClass.None, false) { silent = true };
            Assert.IsFalse(DebuffLeyCharge.ShouldStrike(kill, MaxDepth));
        }

        [Test]
        public void AChainHop_StrikesUntilTheMaxDepth()
        {
            var hop = new DamageRequest(2f, WeaponClass.Magic, false) { leyChargeChainDepth = MaxDepth - 1 };
            Assert.IsTrue(DebuffLeyCharge.ShouldStrike(hop, MaxDepth), "A hop below the max depth carries the chain on");

            hop.leyChargeChainDepth = MaxDepth;
            Assert.IsFalse(DebuffLeyCharge.ShouldStrike(hop, MaxDepth), "The chain stops at the max depth");
        }

        [Test]
        public void DamageOverTime_NeverTriggersLifesteal()
        {
            var tick = DamageRequest.DamageOverTime(2f, WeaponClass.Melee);
            Assert.IsTrue(tick.isDamageOverTime);
            Assert.IsFalse(tick.canTriggerLifeSteal);
            Assert.AreEqual(2f, tick.baseDamage);
            Assert.AreEqual(WeaponClass.Melee, tick.originWeaponClass);
        }

        [TestCase("Assets/Scripts/Combat/Elemental/DebuffFire.cs")]
        [TestCase("Assets/Scripts/Combat/Elemental/DebuffBleed.cs")]
        public void DamageOverTimeDebuffs_MarkTheirDamage(string path)
        {
            // Their ticks must go through DamageRequest.DamageOverTime, or Ley Charge strikes on every tick again
            string source = File.ReadAllText(path);
            StringAssert.Contains("DamageRequest.DamageOverTime(", source);
            StringAssert.DoesNotContain("new DamageRequest(", source);
        }
    }
}
