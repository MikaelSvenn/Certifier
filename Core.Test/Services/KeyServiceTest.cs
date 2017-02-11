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
            public void ShouldCreateValidRsaKeyPair()
            {

            }

            [Test]
            public void ShouldCreate4096BitKeyWhenNoKeySizeIsSpecified()
            {

            }

            [Test]
            public void ShouldCreatePrivateKeyOfGivenSize()
            {

            }

            [Test]
            public void ShouldNotAllowKeySizeBelow4096Bits()
            {

            }

            [Test]
            public void ShouldEncryptPrivateKeyWithGivenPassword()
            {

            }
        }
    }
}