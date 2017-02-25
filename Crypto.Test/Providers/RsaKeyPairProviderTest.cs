using System.Runtime.Remoting;
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
            private IAsymmetricKey keyPair;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = keyProvider.CreateAsymmetricKeyPair(2048);
            }

            [Test]
            public void ShouldCreatePrivateKey()
            {
                Assert.IsNotEmpty(keyPair.PrivateKey);
            }

            [Test]
            public void ShouldCreatePublicKey()
            {
                Assert.IsNotEmpty(keyPair.PublicKey);
            }

            [Test]
            public void ShouldSetKeyLength()
            {
                Assert.AreEqual(2048, keyPair.KeyLengthInBits);
            }

            [Test]
            public void ShouldSetKeyType()
            {
                Assert.AreEqual(AsymmetricKeyType.Rsa, keyPair.KeyType);
            }

            [Test]
            public void ShouldCreateValidUnencryptedKeyPair()
            {
                var privateKey = PrivateKeyFactory.CreateKey(keyPair.PrivateKey);
                var publicKey = PublicKeyFactory.CreateKey(keyPair.PublicKey);

                var privateKeyModulus = ((RsaKeyParameters) privateKey).Modulus;
                var publicKeyModulus = ((RsaKeyParameters) publicKey).Modulus;

                Assert.AreEqual(0, privateKeyModulus.CompareTo(publicKeyModulus));
            }
        }

        [TestFixture]
        public class CreatePkcs12KeyPairPairTest : RsaKeyPairProviderTest
        {
            private IAsymmetricKey keyPair;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = keyProvider.CreateAsymmetricPkcs12KeyPair("foopassword", 2048);
            }

            [Test]
            public void ShouldCreatePrivateKey()
            {
                Assert.IsNotEmpty(keyPair.PrivateKey);
            }

            [Test]
            public void ShouldCreatePublicKey()
            {
                Assert.IsNotEmpty(keyPair.PublicKey);
            }

            [Test]
            public void ShouldSetKeyLength()
            {
                Assert.AreEqual(2048, keyPair.KeyLengthInBits);
            }

            [Test]
            public void ShouldSetKeyType()
            {
                Assert.AreEqual(AsymmetricKeyType.RsaPkcs12, keyPair.KeyType);
            }

            [Test]
            public void ShouldCreateValidEncryptedKeyPair()
            {
                var privateKey = PrivateKeyFactory.DecryptKey("foopassword".ToCharArray(), keyPair.PrivateKey);
                var publicKey = PublicKeyFactory.CreateKey(keyPair.PublicKey);

                var privateKeyModulus = ((RsaKeyParameters) privateKey).Modulus;
                var publicKeyModulus = ((RsaKeyParameters) publicKey).Modulus;

                Assert.AreEqual(0, privateKeyModulus.CompareTo(publicKeyModulus));
            }
        }
    }
}