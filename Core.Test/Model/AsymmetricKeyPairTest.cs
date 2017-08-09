using Core.Model;
using NUnit.Framework;

namespace Core.Test.Model
{
    [TestFixture]
    public class AsymmetricKeyPairTest
    {
        private AsymmetricKeyPair keyPair;
        private RsaKey privateKey;
        private RsaKey publicKey;

        [SetUp]
        public void Setup()
        {
            privateKey = new RsaKey(null, AsymmetricKeyType.Encrypted, 4096);
            publicKey = new RsaKey(null, AsymmetricKeyType.Public, 2048);

            keyPair = new AsymmetricKeyPair(privateKey, publicKey);
        }

        [TestFixture]
        public class HasPassword : AsymmetricKeyPairTest
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