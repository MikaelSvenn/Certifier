using System;
using Core.Interfaces;
using Core.Services;
using Moq;
using NUnit.Framework;

namespace Core.Test.Services
{
    [TestFixture]
    public class KeyServiceTest
    {
        private KeyService keyService;
        private Mock<IKeyProvider> keyProvider;
        private IAsymmetricKey expected;

        [SetUp]
        public void SetupKeyServiceTest()
        {
            expected = Mock.Of<IAsymmetricKey>();
            keyProvider = new Mock<IKeyProvider>();
            keyProvider.Setup(kp => kp.CreateAsymmetricKeyPair("foopassword", 4096))
                .Returns(expected);

            keyService = new KeyService(keyProvider.Object);
        }

        [TestFixture]
        public class CreateRsaKeyPair : KeyServiceTest
        {
            private IAsymmetricKey keyPair;

            [SetUp]
            public void Setup()
            {
                keyPair = keyService.CreateAsymmetricKeyPair("foopassword");
            }

            [Test]
            public void ShouldCreate4096BitKeyWhenNoKeySizeIsSpecified()
            {
                keyProvider.Verify(kp => kp.CreateAsymmetricKeyPair("foopassword", 4096));
            }

            [Test]
            public void ShouldNotAllowKeySizeBelow4096Bits()
            {
                Assert.Throws<ArgumentException>(() => { keyService.CreateAsymmetricKeyPair("foo", 1024); });
            }

            [Test]
            public void ShouldCreateKeyWithGivenKeySize()
            {
                keyPair = keyService.CreateAsymmetricKeyPair("bar", 8192);
                keyProvider.Verify(kp => kp.CreateAsymmetricKeyPair("bar", 8192));
            }

            [Test]
            public void ShouldReturnCreatedKey()
            {
                Assert.AreEqual(expected, keyPair);
            }
        }
    }
}