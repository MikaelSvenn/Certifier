using Core.Model;
using NUnit.Framework;

namespace Core.Test.Model
{
    [TestFixture]
    public class EcKeyTest
    {
        [TestCase(AsymmetricKeyType.Private, TestName = "Private")]
        [TestCase(AsymmetricKeyType.Public, TestName = "Public")]
        public void ShouldHaveEcAsCipherType(AsymmetricKeyType keyType)
        {
            var key = new EcKey(null, keyType, 1);
            Assert.AreEqual(CipherType.Ec, key.CipherType);
        }

        [TestFixture]
        public class IsEncrypted : EcKeyTest
        {
            [TestCase(AsymmetricKeyType.Private, TestName = "Private")]
            [TestCase(AsymmetricKeyType.Public, TestName = "Public")]
            public void ShouldRetrunFalseWhenKeyTypeIsNotEncrypted(AsymmetricKeyType keyType)
            {
                var key = new EcKey(null, keyType, 1);
                Assert.IsFalse(key.IsEncrypted);
            }

            [Test]
            public void ShouldReturnTrueWhenKeyTypeIsEncrypted()
            {
                var key = new EcKey(null, AsymmetricKeyType.Encrypted, 1);
                Assert.IsTrue(key.IsEncrypted);
            }
        }

        [TestFixture]
        public class IsPrivateKey : EcKeyTest
        {
            [Test]
            public void ShouldReturnFalseWhenKeyTypeIsPublic()
            {
                var key = new EcKey(null, AsymmetricKeyType.Public, 1);
                Assert.IsFalse(key.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnTrueWhenKeyTypeIsPrivate()
            {
                var key = new EcKey(null, AsymmetricKeyType.Private, 1);
                Assert.IsTrue(key.IsPrivateKey);
            }
        }
    }
}