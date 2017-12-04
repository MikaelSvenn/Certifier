using System;
using System.Linq;
using Core.Interfaces;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using Crypto.Wrappers;
using Moq;
using NUnit.Framework;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class Pkcs8PemFormattingProviderTest
    {
        private Pkcs8PemFormattingProvider pkcs8PemFormattingProvider;
        private RsaKeyProvider rsaKeyProvider;
        private IAsymmetricKeyPair keyPair;
        private IAsymmetricKey encryptedKey;

        [OneTimeSetUp]
        public void SetupFormattingProviderTest()
        {
            var secureRandom = new SecureRandomGenerator();
            rsaKeyProvider = new RsaKeyProvider(new AsymmetricKeyPairGenerator(secureRandom));
            keyPair = rsaKeyProvider.CreateKeyPair(2048);

            var oidMapper = new OidToCipherTypeMapper();
            var asymmetricKeyConverter = new AsymmetricKeyProvider(oidMapper, new KeyInfoWrapper(), rsaKeyProvider, null, null, null);
            pkcs8PemFormattingProvider = new Pkcs8PemFormattingProvider(asymmetricKeyConverter);

            var configuration = Mock.Of<IConfiguration>(m => m.Get<int>("SaltLengthInBytes") == 100 &&
                                                         m.Get<int>("KeyDerivationIterationCount") == 1);

            var pkcsEncryptionProvider = new Pkcs8EncryptionProvider(configuration, secureRandom, asymmetricKeyConverter, new Pkcs12EncryptionGenerator());
            encryptedKey = pkcsEncryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "password");
        }

        [TestFixture]
        public class GetAsPem : Pkcs8PemFormattingProviderTest
        {
            private string publicKey;
            private string privateKey;
            private string encryptedPrivateKey;

            [SetUp]
            public void Setup()
            {
                publicKey = pkcs8PemFormattingProvider.GetAsPem(keyPair.PublicKey);
                privateKey = pkcs8PemFormattingProvider.GetAsPem(keyPair.PrivateKey);
                encryptedPrivateKey = pkcs8PemFormattingProvider.GetAsPem(encryptedKey);
            }

            [Test]
            public void ShouldFormatPublicKey()
            {
                var result =  publicKey.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual("-----BEGIN PUBLIC KEY-----", result.First());
                Assert.AreEqual("-----END PUBLIC KEY-----", result.Last());
                Assert.IsTrue(result.Length > 3);
            }

            [Test]
            public void ShouldFormatPrivateKey()
            {
                var result =  privateKey.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual("-----BEGIN PRIVATE KEY-----", result.First());
                Assert.AreEqual("-----END PRIVATE KEY-----", result.Last());
                Assert.IsTrue(result.Length > 3);
            }

            [Test]
            public void ShouldFormatEncryptedPrivateKey()
            {
                var result =  encryptedPrivateKey.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual("-----BEGIN ENCRYPTED PRIVATE KEY-----", result.First());
                Assert.AreEqual("-----END ENCRYPTED PRIVATE KEY-----", result.Last());
                Assert.IsTrue(result.Length > 3);
            }
        }

        [TestFixture]
        public class GetAsDer : Pkcs8PemFormattingProviderTest
        {
            private string publicKeyAsPem;
            private string privateKeyAsPem;
            private string encryptedPrivateKeyAsPem;

            [SetUp]
            public void Setup()
            {
                publicKeyAsPem = pkcs8PemFormattingProvider.GetAsPem(keyPair.PublicKey);
                privateKeyAsPem = pkcs8PemFormattingProvider.GetAsPem(keyPair.PrivateKey);
                encryptedPrivateKeyAsPem = pkcs8PemFormattingProvider.GetAsPem(encryptedKey);
            }

            [Test]
            public void ShouldFormatPublicKey()
            {
                var result = pkcs8PemFormattingProvider.GetAsDer(publicKeyAsPem);
                Assert.AreEqual(keyPair.PublicKey.Content, result.Content);
            }

            [Test]
            public void ShouldFormatPrivateKey()
            {
                var result = pkcs8PemFormattingProvider.GetAsDer(privateKeyAsPem);
                Assert.AreEqual(keyPair.PrivateKey.Content, result.Content);
            }

            [Test]
            public void ShouldFormatEncryptedPrivateKey()
            {
                var result = pkcs8PemFormattingProvider.GetAsDer(encryptedPrivateKeyAsPem);
                Assert.AreEqual(encryptedKey.Content, result.Content);
            }
        }
    }
}