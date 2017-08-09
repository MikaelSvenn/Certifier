using Core.Model;
using NUnit.Framework;

namespace Core.Test.Model
{
    [TestFixture]
    public class DsaKeyTest
    {
        [TestCase(AsymmetricKeyType.Private, TestName = "Private")]
        [TestCase(AsymmetricKeyType.Public, TestName = "Public")]
        public void ShouldHaveRsaAsCipherType(AsymmetricKeyType keyType)
        {
            var key = new DsaKey(null, keyType, 1);
            Assert.AreEqual(CipherType.Dsa, key.CipherType);
        }

        [TestFixture]
        public class IsEncrypted : DsaKeyTest
        {
            [TestCase(AsymmetricKeyType.Private, TestName = "Private")]
            [TestCase(AsymmetricKeyType.Public, TestName = "Public")]
            public void ShouldRetrunFalseWhenKeyTypeIsNotEncrypted(AsymmetricKeyType keyType)
            {
                var key = new DsaKey(null, keyType, 1);
                Assert.IsFalse(key.IsEncrypted);
            }

            [Test]
            public void ShouldReturnTrueWhenKeyTypeIsEncrypted()
            {
                var key = new DsaKey(null, AsymmetricKeyType.Encrypted, 1);
                Assert.IsTrue(key.IsEncrypted);
            }
        }

        [TestFixture]
        public class IsPrivateKey : DsaKeyTest
        {
            [Test]
            public void ShouldReturnFalseWhenKeyTypeIsPublic()
            {
                var key = new DsaKey(null, AsymmetricKeyType.Public, 1);
                Assert.IsFalse(key.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnTrueWhenKeyTypeIsPrivate()
            {
                var key = new DsaKey(null, AsymmetricKeyType.Private, 1);
                Assert.IsTrue(key.IsPrivateKey);
            }
        }
    }
}