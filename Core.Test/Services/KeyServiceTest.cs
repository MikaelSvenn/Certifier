using System;
using Core.Model;
using Core.Services;
using NUnit.Framework;

namespace Core.Test.Services
{
    [TestFixture]
    public class KeyServiceTest
    {
        private KeyService keyService;

        [SetUp]
        public void SetupKeyServiceTest()
        {
            keyService = new KeyService();
        }

        [TestFixture]
        public class CreateRsaKeyPair : KeyServiceTest
        {
            private RsaKeyPair keyPair;

            [SetUp]
            public void Setup()
            {
                keyPair = keyService.CreateRsaKeyPair("foo");
            }

            [Test]
            public void ShouldCreateRsaKeyPairWithPrivateKey()
            {
                Assert.IsNotEmpty(keyPair.PrivateKey);
            }

            [Test]
            public void ShouldCreateRsaKeyPairWithPublicKey()
            {
                Assert.IsNotEmpty(keyPair.PublicKey);
            }

            [Test]
            public void ShouldCreate4096BitKeyWhenNoKeySizeIsSpecified()
            {
                Assert.AreEqual(4096, keyPair.KeyLengthInBits);
            }

            [Test]
            public void ShouldCreatePrivateKeyOfGivenSize()
            {
                keyPair = keyService.CreateRsaKeyPair("foo", 8192);
                Assert.AreEqual(8192, keyPair.KeyLengthInBits);
            }

            [Test]
            public void ShouldNotAllowKeySizeBelow4096Bits()
            {
                Assert.Throws<InvalidOperationException>(() => { keyService.CreateRsaKeyPair("foo", 1024); });
            }

            [Test]
            public void ShouldEncryptPrivateKeyWithGivenPassword()
            {

            }
        }
    }
}