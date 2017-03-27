using Core.Model;
using NUnit.Framework;

namespace Core.Test.Model
{
    [TestFixture]
    public class RsaKeyTest
    {
        [TestCase(AsymmetricKeyType.Private, TestName = "Private")]
        [TestCase(AsymmetricKeyType.Public, TestName = "Public")]
        public void ShouldHaveRsaAsCipherType(AsymmetricKeyType keyType)
        {
            var key = new RsaKey(null, keyType, 1);
            Assert.AreEqual(CipherType.Rsa, key.CipherType);
        }

        [TestFixture]
        public class IsEncrypted : RsaKeyTest
        {
            [TestCase(AsymmetricKeyType.Private, TestName = "Private")]
            [TestCase(AsymmetricKeyType.Public, TestName = "Public")]
            public void ShouldRetrunFalseWhenKeyTypeIsNotEncrypted(AsymmetricKeyType keyType)
            {
                var key = new RsaKey(null, keyType, 1);
                Assert.IsFalse(key.IsEncrypted);
            }

            [Test]
            public void ShouldReturnTrueWhenKeyTypeIsEncrypted()
            {
                var key = new RsaKey(null, AsymmetricKeyType.Encrypted, 1);
                Assert.IsTrue(key.IsEncrypted);
            }
        }

        [TestFixture]
        public class IsPrivateKey : RsaKeyTest
        {
            [Test]
            public void ShouldReturnFalseWhenKeyTypeIsRsaPublic()
            {
                var key = new RsaKey(null, AsymmetricKeyType.Public, 1);
                Assert.IsFalse(key.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnTrueWhenKeyTypeIsRsaPrivate()
            {
                var key = new RsaKey(null, AsymmetricKeyType.Private, 1);
                Assert.IsTrue(key.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnTrueWhenKeyTypeIsRsaPkcs12()
            {
                var key = new RsaKey(null, AsymmetricKeyType.Encrypted, 1);
                Assert.IsTrue(key.IsPrivateKey);
            }
        }
    }
}