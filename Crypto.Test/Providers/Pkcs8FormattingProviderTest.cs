using System;
using System.Linq;
using Core.Configuration;
using Core.Interfaces;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using NUnit.Framework;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class Pkcs8FormattingProviderTest
    {
        private Pkcs8FormattingProvider pkcs8FormattingProvider;
        private RsaKeyProvider rsaKeyProvider;
        private IAsymmetricKeyPair unencryptedKey;
        private IAsymmetricKeyPair encryptedKeyPair;

        [OneTimeSetUp]
        public void SetupFormattingProviderTest()
        {
            var secureRandom = new SecureRandomGenerator();
            rsaKeyProvider = new RsaKeyProvider(new PbeConfiguration(), new RsaKeyPairGenerator(secureRandom), secureRandom);
            unencryptedKey = rsaKeyProvider.CreateKeyPair(2048);
            encryptedKeyPair = rsaKeyProvider.CreatePkcs12KeyPair("password", 2048);

            var oidMapper = new OidToCipherTypeMapper();
            var asymmetricKeyConverter = new AsymmetricKeyProvider(oidMapper, rsaKeyProvider);
            pkcs8FormattingProvider = new Pkcs8FormattingProvider(asymmetricKeyConverter);
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
                publicKey = pkcs8FormattingProvider.GetAsPem(unencryptedKey.PublicKey);
                privateKey = pkcs8FormattingProvider.GetAsPem(unencryptedKey.PrivateKey);
                encryptedPrivateKey = pkcs8FormattingProvider.GetAsPem(encryptedKeyPair.PrivateKey);
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
                publicKeyAsPem = pkcs8FormattingProvider.GetAsPem(unencryptedKey.PublicKey);
                privateKeyAsPem = pkcs8FormattingProvider.GetAsPem(unencryptedKey.PrivateKey);
                encryptedPrivateKeyAsPem = pkcs8FormattingProvider.GetAsPem(encryptedKeyPair.PrivateKey);
            }

            [Test]
            public void ShouldFormatPublicKey()
            {
                var result = pkcs8FormattingProvider.GetAsDer(publicKeyAsPem);
                Assert.AreEqual(unencryptedKey.PublicKey.Content, result.Content);
            }

            [Test]
            public void ShouldFormatPrivateKey()
            {
                var result = pkcs8FormattingProvider.GetAsDer(privateKeyAsPem);
                Assert.AreEqual(unencryptedKey.PrivateKey.Content, result.Content);
            }

            [Test]
            public void ShouldFormatEncryptedPrivateKey()
            {
                var result = pkcs8FormattingProvider.GetAsDer(encryptedPrivateKeyAsPem);
                Assert.AreEqual(encryptedKeyPair.PrivateKey.Content, result.Content);
            }
        }
    }
}