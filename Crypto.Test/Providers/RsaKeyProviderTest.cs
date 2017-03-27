using System;
using System.Threading;
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
    public class RsaKeyProviderTest
    {
        private RsaKeyProvider keyProvider;

        [OneTimeSetUp]
        public void SetupRsaKeyProviderTest()
        {
            var config = Mock.Of<IConfiguration>(m => m.Get<int>("KeyDerivationIterationCount") == 10 &&
                m.Get<int>("SaltLengthInBytes") == 100);

            var secureRandomGenerator = new SecureRandomGenerator();
            var rsaGenerator = new RsaKeyPairGenerator(secureRandomGenerator);

            keyProvider = new RsaKeyProvider(config, rsaGenerator, secureRandomGenerator);
        }

        [TestFixture]
        public class CreateKeyPairTest : RsaKeyProviderTest
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
                Assert.AreEqual(AsymmetricKeyType.Private, keyPair.PrivateKey.KeyType);
            }

            [Test]
            public void ShouldSetPublicKeyType()
            {
                Assert.AreEqual(AsymmetricKeyType.Public, keyPair.PublicKey.KeyType);
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
        public class CreatePkcs12KeyTest : RsaKeyProviderTest
        {
            private IAsymmetricKeyPair keyPair;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = keyProvider.CreatePkcs12KeyPair("foopassword", 2048);
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
                Assert.AreEqual(AsymmetricKeyType.Encrypted, keyPair.PrivateKey.KeyType);
            }

            [Test]
            public void ShouldSetPublicKeyType()
            {
                Assert.AreEqual(AsymmetricKeyType.Public, keyPair.PublicKey.KeyType);
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

        [TestFixture]
        public class GetKeyTest : RsaKeyProviderTest
        {
            private IAsymmetricKeyPair keyPair;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = keyProvider.CreateKeyPair(1024);
            }

            [Test]
            public void ShouldThrowExceptionWhenKeyIsEncrypted()
            {
                Assert.Throws<InvalidOperationException>(() => keyProvider.GetKey(null, AsymmetricKeyType.Encrypted));
            }

            [Test]
            public void ShouldSetPublicKeyLength()
            {
                var result = keyProvider.GetKey(keyPair.PublicKey.Content, AsymmetricKeyType.Public);
                Assert.AreEqual(1024, result.KeySize);
            }

            [Test]
            public void ShouldSetPrivateKeyLength()
            {
                var result = keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Private);
                Assert.AreEqual(1024, result.KeySize);
            }

            [Test]
            public void ShouldReturnPublicRsaKey()
            {
                var result = keyProvider.GetKey(keyPair.PublicKey.Content, AsymmetricKeyType.Public);
                Assert.IsAssignableFrom<RsaKey>(result);
            }

            [Test]
            public void ShouldReturnPrivateRsaKey()
            {
                var result = keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Private);
                Assert.IsAssignableFrom<RsaKey>(result);
            }

            [Test]
            public void ShouldThrowExceptionWhenWrongKeyTypeIsProvided()
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Public);
                });
            }
        }
    }
}