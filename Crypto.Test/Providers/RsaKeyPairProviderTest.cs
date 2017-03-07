using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Providers;
using Moq;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class RsaKeyPairProviderTest
    {
        private RsaKeyPairProvider keyProvider;

        [OneTimeSetUp]
        public void SetupRsaKeyProviderTest()
        {
            var config = Mock.Of<IConfiguration>(m => m.Get<int>("KeyDerivationIterationCount") == 10);
            var secureRandomGenerator = new SecureRandomGenerator();
            var rsaGenerator = new RsaKeyPairGenerator(secureRandomGenerator);

            keyProvider = new RsaKeyPairProvider(config, rsaGenerator, secureRandomGenerator);
        }

        [TestFixture]
        public class CreateAsymmetricKeyPairTest : RsaKeyPairProviderTest
        {
            private IAsymmetricKeyPair keyPair;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = keyProvider.CreateAsymmetricKeyPair(2048);
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
            public void ShouldSetPrivateKeyLength()
            {
                Assert.AreEqual(2048, keyPair.PrivateKey.KeySize);
            }

            [Test]
            public void ShouldSetPublicKeyLength()
            {
                Assert.AreEqual(2048, keyPair.PublicKey.KeySize);
            }

            [Test]
            public void ShouldSetPrivateKeyType()
            {
                Assert.AreEqual(AsymmetricKeyType.Rsa, keyPair.PrivateKey.KeyType);
            }

            [Test]
            public void ShouldSetPublicKeyType()
            {
                Assert.AreEqual(AsymmetricKeyType.Rsa, keyPair.PublicKey.KeyType);
            }

            [Test]
            public void ShouldCreateValidUnencryptedKeyPair()
            {
                var privateKey = PrivateKeyFactory.CreateKey(keyPair.PrivateKey.Content);
                var publicKey = PublicKeyFactory.CreateKey(keyPair.PublicKey.Content);

                var privateKeyModulus = ((RsaKeyParameters) privateKey).Modulus;
                var publicKeyModulus = ((RsaKeyParameters) publicKey).Modulus;

                Assert.AreEqual(0, privateKeyModulus.CompareTo(publicKeyModulus));
            }
        }

        [TestFixture]
        public class CreatePkcs12KeyPairPairTest : RsaKeyPairProviderTest
        {
            private IAsymmetricKeyPair keyPair;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = keyProvider.CreateAsymmetricPkcs12KeyPair("foopassword", 2048);
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
            public void ShouldSetPrivateKeyLength()
            {
                Assert.AreEqual(2048, keyPair.PrivateKey.KeySize);
            }

            [Test]
            public void ShouldSetPublicKeyLength()
            {
                Assert.AreEqual(2048, keyPair.PublicKey.KeySize);
            }

            [Test]
            public void ShouldSetPrivateKeyType()
            {
                Assert.AreEqual(AsymmetricKeyType.RsaPkcs12, keyPair.PrivateKey.KeyType);
            }

            [Test]
            public void ShouldSetPublicKeyType()
            {
                Assert.AreEqual(AsymmetricKeyType.Rsa, keyPair.PublicKey.KeyType);
            }

            [Test]
            public void ShouldCreateValidEncryptedKeyPair()
            {
                var privateKey = PrivateKeyFactory.DecryptKey("foopassword".ToCharArray(), keyPair.PrivateKey.Content);
                var publicKey = PublicKeyFactory.CreateKey(keyPair.PublicKey.Content);

                var privateKeyModulus = ((RsaKeyParameters) privateKey).Modulus;
                var publicKeyModulus = ((RsaKeyParameters) publicKey).Modulus;

                Assert.AreEqual(0, privateKeyModulus.CompareTo(publicKeyModulus));
            }
        }
    }
}