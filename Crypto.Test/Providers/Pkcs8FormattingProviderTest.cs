using System;
using System.Linq;
using Core.Configuration;
using Core.Interfaces;
using Crypto.Generators;
using Crypto.Providers;
using NUnit.Framework;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class Pkcs8FormattingProviderTest
    {
        private Pkcs8FormattingProvider pkcs8FormattingProvider;
        private RsaKeyPairProvider rsaKeyProvider;
        private IAsymmetricKeyPair unencryptedKey;
        private IAsymmetricKeyPair encryptedKeyPair;

        [SetUp]
        public void SetupFormattingProviderTest()
        {
            pkcs8FormattingProvider = new Pkcs8FormattingProvider();

            var secureRandom = new SecureRandomGenerator();
            rsaKeyProvider = new RsaKeyPairProvider(new KeyProviderConfiguration(), new RsaKeyPairGenerator(secureRandom), secureRandom);
            unencryptedKey = rsaKeyProvider.CreateAsymmetricKeyPair(2048);
            encryptedKeyPair = rsaKeyProvider.CreateAsymmetricPkcs12KeyPair("password", 2048);
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
                var result =  publicKey.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                Assert.AreEqual(result.First(), "-----BEGIN PUBLIC KEY-----");
                Assert.AreEqual(result.Last(), "-----END PUBLIC KEY-----");
                Assert.IsTrue(result.Length >= 3);
            }

            [Test]
            public void ShouldFormatPrivateKey()
            {
                var result =  privateKey.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                Assert.AreEqual(result.First(), "-----BEGIN PRIVATE KEY-----");
                Assert.AreEqual(result.Last(), "-----END PRIVATE KEY-----");
                Assert.IsTrue(result.Length >= 3);
            }

            [Test]
            public void ShouldFormatEncryptedPrivateKey()
            {
                var result =  encryptedPrivateKey.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                Assert.AreEqual(result.First(), "-----BEGIN ENCRYPTED PRIVATE KEY-----");
                Assert.AreEqual(result.Last(), "-----END ENCRYPTED PRIVATE KEY-----");
                Assert.IsTrue(result.Length >= 3);
            }

            [Test]
            public void ShouldReturnValidPemFormattedObject()
            {

            }
        }

        [TestFixture]
        public class GetAsDer : Pkcs8FormattingProviderTest
        {
            [SetUp]
            public void Setup()
            {

            }

            [Test]
            public void ShouldFormatPemObjectAsDer()
            {

            }

            [Test]
            public void ShouldReturnValidDerFormattedObject()
            {

            }
        }
    }
}