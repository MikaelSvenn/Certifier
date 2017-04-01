using System;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using Moq;
using NUnit.Framework;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class AsymmetricKeyProviderTest
    {
        private AsymmetricKeyProvider keyProvider;
        private RsaKeyProvider rsaKeyProvider;
        private Mock<OidToCipherTypeMapper> cipherTypeMapper;
        private PkcsEncryptionProvider pkcsEncryptionProvider;

        [SetUp]
        public void SetupAsymmetricKeyProviderTest()
        {
            var configuration = Mock.Of<IConfiguration>(c => c.Get<int>("SaltLengthInBytes") == 100 &&
                                                             c.Get<int>("KeyDerivationIterationCount") == 10);
            var secureRandom = new SecureRandomGenerator();
            var rsaGenerator = new RsaKeyPairGenerator(secureRandom);
            rsaKeyProvider = new RsaKeyProvider(rsaGenerator);

            cipherTypeMapper = new Mock<OidToCipherTypeMapper>();
            keyProvider = new AsymmetricKeyProvider(cipherTypeMapper.Object, rsaKeyProvider);

            pkcsEncryptionProvider = new PkcsEncryptionProvider(configuration, secureRandom, keyProvider, new PkcsEncryptionGenerator());
        }

        [TestFixture]
        public class GetPublicKey : AsymmetricKeyProviderTest
        {
            private IAsymmetricKeyPair keyPair;

            [SetUp]
            public void Setup()
            {
                keyPair = rsaKeyProvider.CreateKeyPair(2048);
                cipherTypeMapper.Setup(ctm => ctm.MapOidToCipherType(It.IsAny<string>()))
                    .Returns(CipherType.Rsa);
            }

            [Test]
            public void ShouldThrowExceptionWhenKeyIsNotRsa()
            {
                cipherTypeMapper.Setup(ctm => ctm.MapOidToCipherType(It.IsAny<string>()))
                    .Returns(CipherType.Ec);

                Assert.Throws<ArgumentException>(() =>
                {
                    keyProvider.GetPublicKey(keyPair.PublicKey.Content);
                });
            }

            [Test]
            public void ShouldReturnPublicRsaKey()
            {
                IAsymmetricKey result = keyProvider.GetPublicKey(keyPair.PublicKey.Content);

                Assert.IsAssignableFrom<RsaKey>(result);
                Assert.IsFalse(result.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnValidKey()
            {
                var secureRandom = new SecureRandomGenerator();
                var signatureProvider = new SignatureProvider(new SignatureAlgorithmProvider(secureRandom));
                var data = secureRandom.NextBytes(100);

                Signature signature = signatureProvider.CreateSignature(keyPair.PrivateKey, data);
                IAsymmetricKey result = keyProvider.GetPublicKey(keyPair.PublicKey.Content);

                Assert.IsTrue(signatureProvider.VerifySignature(result, signature));
            }
        }

        [TestFixture]
        public class GetPrivateKey : AsymmetricKeyProviderTest
        {
            private IAsymmetricKeyPair keyPair;

            [SetUp]
            public void Setup()
            {
                keyPair = rsaKeyProvider.CreateKeyPair(2048);
                cipherTypeMapper.Setup(ctm => ctm.MapOidToCipherType(It.IsAny<string>()))
                    .Returns(CipherType.Rsa);
            }

            [Test]
            public void ShouldThrowExceptionWhenKeyIsNotRsa()
            {
                cipherTypeMapper.Setup(ctm => ctm.MapOidToCipherType(It.IsAny<string>()))
                    .Returns(CipherType.Dsa);

                Assert.Throws<ArgumentException>(() =>
                {
                    keyProvider.GetPrivateKey(keyPair.PrivateKey.Content);
                });
            }

            [Test]
            public void ShouldReturnPrivateRsaKey()
            {
                IAsymmetricKey result = keyProvider.GetPrivateKey(keyPair.PrivateKey.Content);

                Assert.IsAssignableFrom<RsaKey>(result);
                Assert.IsTrue(result.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnValidKey()
            {
                var secureRandom = new SecureRandomGenerator();
                var signatureProvider = new SignatureProvider(new SignatureAlgorithmProvider(secureRandom));

                var data = secureRandom.NextBytes(100);

                IAsymmetricKey result = keyProvider.GetPrivateKey(keyPair.PrivateKey.Content);
                Signature signature = signatureProvider.CreateSignature(result, data);

                Assert.IsTrue(signatureProvider.VerifySignature(keyPair.PublicKey, signature));
            }
        }

        [TestFixture]
        public class GetEncryptedPrivateKey : AsymmetricKeyProviderTest
        {
            private IAsymmetricKeyPair keyPair;
            private IAsymmetricKey encryptedPrivateKey;

            [SetUp]
            public void Setup()
            {
                keyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), rsaKeyProvider);
                keyPair = rsaKeyProvider.CreateKeyPair(2048);
                var key = pkcsEncryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "foobar");

                encryptedPrivateKey = keyProvider.GetEncryptedPrivateKey(key.Content);
            }

            [Test]
            public void ShouldReturnEncryptedPrivateKey()
            {
                Assert.IsAssignableFrom<EncryptedKey>(encryptedPrivateKey);
                CollectionAssert.AreNotEqual(encryptedPrivateKey.Content, keyPair.PrivateKey.Content);
            }

            [Test]
            public void ShouldSetCipherType()
            {
                Assert.AreEqual(CipherType.Pkcs12Encrypted, encryptedPrivateKey.CipherType);
            }
        }
    }
}