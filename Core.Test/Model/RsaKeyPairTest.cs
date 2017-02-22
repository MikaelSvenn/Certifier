using Core.Model;
using NUnit.Framework;

namespace Core.Test.Model
{
    [TestFixture]
    public class RsaKeyPairTest
    {
        private RsaKeyPair keyPair;

        [SetUp]
        public void Setup()
        {
            keyPair = new RsaKeyPair(null, null, 2048, AsymmetricKeyType.Rsa);
        }

        [TestFixture]
        public class IsEncryptedPrivateKeyTest : RsaKeyPairTest
        {
            [Test]
            public void ShouldReturnFalseWhenKeyTypeIsRsa()
            {
                Assert.IsFalse(keyPair.IsEncryptedPrivateKey);
            }

            [Test]
            public void ShouldReturnTrueWhenKeyTypeIsRsaPkcs12()
            {
                keyPair = new RsaKeyPair(null, null, 2048, AsymmetricKeyType.RsaPkcs12);
                Assert.IsTrue(keyPair.IsEncryptedPrivateKey);
            }
        }

        [TestFixture]
        public class HasPassword : RsaKeyPairTest
        {
            [Test]
            public void ShouldReturnFalseWhenPasswordIsNull()
            {
                Assert.IsFalse(keyPair.HasPassword);
            }

            [Test]
            public void ShouldReturnFalseWhenPasswordIsEmpty()
            {
                keyPair.Password = "";
                Assert.IsFalse(keyPair.HasPassword);
            }

            [Test]
            public void ShouldReturnTrueWhenPasswordIsNotNullOrEmpty()
            {
                keyPair.Password = " ";
                Assert.IsTrue(keyPair.HasPassword);
            }
        }
    }
}