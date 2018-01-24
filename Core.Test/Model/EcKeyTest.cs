using Core.Model;
using NUnit.Framework;

namespace Core.Test.Model
{
    [TestFixture]
    public class EcKeyTest
    {
        private EcKey key;

        [Test]
        public void IsCurve25519ShouldBeTrueWhenCurveIs25519()
        {
            key = new EcKey(null, AsymmetricKeyType.Private, 1, "curve25519");
            Assert.IsTrue(key.IsCurve25519);
        }
        
        [Test]
        public void IsCurve25519ShouldBeFalseWhenCurveIsNot25519()
        {
            key = new EcKey(null, AsymmetricKeyType.Private, 1, "P-256");
            Assert.IsFalse(key.IsCurve25519);
        }
    }
}