using Core.Model;
using NUnit.Framework;

namespace Core.Test.Model
{
    [TestFixture]
    public class EncryptedKeyTest
    {
        private EncryptedKey key;

        [SetUp]
        public void Setup()
        {
            key = new EncryptedKey(new byte[]{0x01}, CipherType.Pkcs5Encrypted);
        }

        [Test]
        public void ShouldSetGivenContent()
        {
            CollectionAssert.AreEqual(new byte[]{0x01}, key.Content);
        }

        [Test]
        public void ShouldSetCipherType()
        {
            Assert.AreEqual(CipherType.Pkcs5Encrypted, key.CipherType);
        }

        [Test]
        public void ShouldHaveEncryptedAsymmetricKeyType()
        {
            Assert.AreEqual(AsymmetricKeyType.Encrypted, key.KeyType);
        }

        [Test]
        public void ShouldHaveKeySizeOf0()
        {
            Assert.AreEqual(0, key.KeySize);
        }

        [Test]
        public void ShouldBeEncrypted()
        {
            Assert.IsTrue(key.IsEncrypted);
        }

        [Test]
        public void ShouldBePrivateKey()
        {
            Assert.IsTrue(key.IsPrivateKey);
        }
    }
}