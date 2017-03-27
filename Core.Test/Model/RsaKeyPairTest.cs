using Core.Model;
using NUnit.Framework;

namespace Core.Test.Model
{
    [TestFixture]
    public class RsaKeyPairTest
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

        [Test]
        public void KeyLengthInBitsShouldReturnPrivateKeyLength()
        {
            Assert.AreEqual(4096, keyPair.KeyLengthInBits);
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