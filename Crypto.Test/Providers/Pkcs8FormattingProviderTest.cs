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
    public class Pkcs8FormattingProviderTest
    {
        private Pkcs8FormattingProvider pkcs8FormattingProvider;
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
            var asymmetricKeyConverter = new AsymmetricKeyProvider(oidMapper, new KeyInfoWrapper(), rsaKeyProvider, null);
            pkcs8FormattingProvider = new Pkcs8FormattingProvider(asymmetricKeyConverter);

            var configuration = Mock.Of<IConfiguration>(m => m.Get<int>("SaltLengthInBytes") == 100 &&
                                                         m.Get<int>("KeyDerivationIterationCount") == 1);

            var pkcsEncryptionProvider = new PkcsEncryptionProvider(configuration, secureRandom, asymmetricKeyConverter, new PkcsEncryptionGenerator());
            encryptedKey = pkcsEncryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "password");
        }

        [TestFixture]
        public class GetAsPem : Pkcs8FormattingProviderTest
        {
            private string publicKey;
            private string privateKey;
            private string encryptedPrivateKey;

            [SetUp]
            public void Setup()
            {
                publicKey = pkcs8FormattingProvider.GetAsPem(keyPair.PublicKey);
                privateKey = pkcs8FormattingProvider.GetAsPem(keyPair.PrivateKey);
                encryptedPrivateKey = pkcs8FormattingProvider.GetAsPem(encryptedKey);
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
        public class GetAsDer : Pkcs8FormattingProviderTest
        {
            private string publicKeyAsPem;
            private string privateKeyAsPem;
            private string encryptedPrivateKeyAsPem;

            [SetUp]
            public void Setup()
            {
                publicKeyAsPem = pkcs8FormattingProvider.GetAsPem(keyPair.PublicKey);
                privateKeyAsPem = pkcs8FormattingProvider.GetAsPem(keyPair.PrivateKey);
                encryptedPrivateKeyAsPem = pkcs8FormattingProvider.GetAsPem(encryptedKey);
            }

            [Test]
            public void ShouldFormatPublicKey()
            {
                var result = pkcs8FormattingProvider.GetAsDer(publicKeyAsPem);
                Assert.AreEqual(keyPair.PublicKey.Content, result.Content);
            }

            [Test]
            public void ShouldFormatPrivateKey()
            {
                var result = pkcs8FormattingProvider.GetAsDer(privateKeyAsPem);
                Assert.AreEqual(keyPair.PrivateKey.Content, result.Content);
            }

            [Test]
            public void ShouldFormatEncryptedPrivateKey()
            {
                var result = pkcs8FormattingProvider.GetAsDer(encryptedPrivateKeyAsPem);
                Assert.AreEqual(encryptedKey.Content, result.Content);
            }
        }
    }
}