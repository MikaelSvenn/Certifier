using System;
using System.Linq;
using Core.Interfaces;
using Core.Model;
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
        private IAsymmetricKey pkcsEncryptedKey;
        private IAsymmetricKey aesEncryptedKey;

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

            var encryptionProvider = new KeyEncryptionProvider(configuration, secureRandom, asymmetricKeyConverter, new Pkcs12KeyEncryptionGenerator(), new AesKeyEncryptionGenerator());
            pkcsEncryptedKey = encryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "password", EncryptionType.Pkcs);
            aesEncryptedKey = encryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "password", EncryptionType.Aes);
        }

        [TestFixture]
        public class GetAsPem : Pkcs8PemFormattingProviderTest
        {
            private string publicKey;
            private string privateKey;
            private string pkcsEncryptedPrivateKey;
            private string aesEncryptedPrivateKey;

            [SetUp]
            public void Setup()
            {
                publicKey = pkcs8PemFormattingProvider.GetAsPem(keyPair.PublicKey);
                privateKey = pkcs8PemFormattingProvider.GetAsPem(keyPair.PrivateKey);
                pkcsEncryptedPrivateKey = pkcs8PemFormattingProvider.GetAsPem(pkcsEncryptedKey);
                aesEncryptedPrivateKey = pkcs8PemFormattingProvider.GetAsPem(aesEncryptedKey);
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
            public void ShouldFormatPkcsEncryptedPrivateKey()
            {
                var result =  pkcsEncryptedPrivateKey.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual("-----BEGIN ENCRYPTED PRIVATE KEY-----", result.First());
                Assert.AreEqual("-----END ENCRYPTED PRIVATE KEY-----", result.Last());
                Assert.IsTrue(result.Length > 3);
            }
            
            [Test]
            public void ShouldFormatAesEncryptedPrivateKey()
            {
                var result =  aesEncryptedPrivateKey.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
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
            private string pkcsEncryptedPrivateKeyAsPem;
            private string aesEncryptedPrivateKeyAsPem;

            [SetUp]
            public void Setup()
            {
                publicKeyAsPem = pkcs8PemFormattingProvider.GetAsPem(keyPair.PublicKey);
                privateKeyAsPem = pkcs8PemFormattingProvider.GetAsPem(keyPair.PrivateKey);
                pkcsEncryptedPrivateKeyAsPem = pkcs8PemFormattingProvider.GetAsPem(pkcsEncryptedKey);
                aesEncryptedPrivateKeyAsPem = pkcs8PemFormattingProvider.GetAsPem(aesEncryptedKey);
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
                var result = pkcs8PemFormattingProvider.GetAsDer(aesEncryptedPrivateKeyAsPem);
                Assert.AreEqual(aesEncryptedKey.Content, result.Content);
            }
        }
    }
}