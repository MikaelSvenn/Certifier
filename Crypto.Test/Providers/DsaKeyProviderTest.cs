using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Providers;
using NUnit.Framework;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class DsaKeyProviderTest
    {
        private DsaKeyProvider keyProvider;
        
        [OneTimeSetUp]
        public void SetupDsaKeyProviderTest()
        {
            var secureRandomGenerator = new SecureRandomGenerator();
            var keyGenerator = new AsymmetricKeyPairGenerator(secureRandomGenerator);

            keyProvider = new DsaKeyProvider(keyGenerator);
        }

        [TestFixture]
        public class CreateDsaKeyTest : DsaKeyProviderTest
        {
            private IAsymmetricKeyPair keyPair;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = keyProvider.CreateKeyPair(2048);
            }
            
            [Test]
            public void ShouldCreatePrivateKey()
            {
                Assert.IsNotEmpty(keyPair.PrivateKey.Content);
            }

            [Test]
            public void ShouldCreatePublicKey()
            {
                Assert.IsNotEmpty(keyPair.PublicKey.Content);
            }

            [Test]
            public void ShouldSetPrivateKeySize()
            {
                Assert.AreEqual(2048, keyPair.PrivateKey.KeySize);
            }

            [Test]
            public void ShouldSetPublicKeySize()
            {
                Assert.AreEqual(2048, keyPair.PublicKey.KeySize);
            }

            [Test]
            public void ShouldMarkPrivateKeyAsPrivate()
            {
                Assert.IsTrue(keyPair.PrivateKey.IsPrivateKey);
            }

            [Test]
            public void ShouldNotMarkPublicKeyAsPrivate()
            {
                Assert.IsFalse(keyPair.PublicKey.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnPrivateDsaKey()
            {
                Assert.IsAssignableFrom<DsaKey>(keyPair.PrivateKey);
            }

            [Test]
            public void ShouldReturnPublicDsaKey()
            {
                Assert.IsAssignableFrom<DsaKey>(keyPair.PublicKey);
            }

            [Test]
            public void ShouldCreateValidUnencryptedKeyPair()
            {
                Assert.IsTrue(keyProvider.VerifyKeyPair(keyPair));
            }
        }
    }
}