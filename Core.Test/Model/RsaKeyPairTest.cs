using System.Security.Cryptography;
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
            keyPair = new RsaKeyPair();
        }

        [Test]
        public void KeyLengthInBitsShouldReturnPrivateKeyLengthInBits()
        {
            keyPair.PrivateKey = new byte[] {0x01, 0x02};
            Assert.AreEqual(16, keyPair.KeyLengthInBits);
        }
    }
}