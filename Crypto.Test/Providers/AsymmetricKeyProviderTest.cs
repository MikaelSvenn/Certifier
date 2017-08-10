using System;
using System.Security.Cryptography;
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
    public class AsymmetricKeyProviderTest
    {
        private AsymmetricKeyProvider keyProvider;
        private RsaKeyProvider rsaKeyProvider;
        private DsaKeyProvider dsaKeyProvider;
        private OidToCipherTypeMapper cipherTypeMapper;
        private PkcsEncryptionProvider pkcsEncryptionProvider;
        private Mock<KeyInfoWrapper> keyInfoWrapper;
        private IAsymmetricKeyPair rsaKeyPair;
        private IAsymmetricKeyPair dsaKeyPair;
        
        [OneTimeSetUp]
        public void SetupAsymmetricKeyProviderTest()
        {
            var configuration = Mock.Of<IConfiguration>(c => c.Get<int>("SaltLengthInBytes") == 100 &&
                                                             c.Get<int>("KeyDerivationIterationCount") == 10);
            var secureRandom = new SecureRandomGenerator();
            var asymmetricKeyPairGenerator = new AsymmetricKeyPairGenerator(secureRandom);
            rsaKeyProvider = new RsaKeyProvider(asymmetricKeyPairGenerator);
            dsaKeyProvider = new DsaKeyProvider(asymmetricKeyPairGenerator);
            
            cipherTypeMapper = new OidToCipherTypeMapper();
            keyInfoWrapper = new Mock<KeyInfoWrapper>();
            
            SetupValidKeyInfo();
            SetupValidKeyProvider();
            
            pkcsEncryptionProvider = new PkcsEncryptionProvider(configuration, secureRandom, keyProvider, new PkcsEncryptionGenerator());
            
            rsaKeyPair = rsaKeyProvider.CreateKeyPair(2048);
            dsaKeyPair = dsaKeyProvider.CreateKeyPair(2048);
        }

        private void SetupValidKeyProvider()
        {
            keyProvider = new AsymmetricKeyProvider(cipherTypeMapper, keyInfoWrapper.Object, rsaKeyProvider, dsaKeyProvider);
        }

        private void SetupValidKeyInfo()
        {
            keyInfoWrapper.Setup(k => k.GetPublicKeyInfo(It.IsAny<byte[]>()))
                           .Returns<byte[]>(content =>
                           {
                               var wrapper = new KeyInfoWrapper();
                               return wrapper.GetPublicKeyInfo(content);
                           });

            keyInfoWrapper.Setup(k => k.GetPrivateKeyInfo(It.IsAny<byte[]>()))
                           .Returns<byte[]>(content =>
                           {
                               var wrapper = new KeyInfoWrapper();
                               return wrapper.GetPrivateKeyInfo(content);
                           });
        }

        [TestFixture]
        public class GetPublicKey : AsymmetricKeyProviderTest
        {
            [SetUp]
            public void Setup()
            {
                SetupValidKeyProvider();
                SetupValidKeyInfo();
            }
            
            [TestCase(CipherType.Ec)]
            [TestCase(CipherType.ElGamal)]
            [TestCase(CipherType.Pkcs5Encrypted)]
            [TestCase(CipherType.Pkcs12Encrypted)]
            [TestCase(CipherType.Unknown)]
            public void ShouldThrowExceptionWhenKeyTypeIsNotSupported(CipherType cipherType)
            {
                var typeMapperMock = new Mock<OidToCipherTypeMapper>();
                typeMapperMock.Setup(ctm => ctm.MapOidToCipherType(It.IsAny<string>()))
                                .Returns(cipherType);

                keyProvider = new AsymmetricKeyProvider(typeMapperMock.Object, keyInfoWrapper.Object, rsaKeyProvider, dsaKeyProvider);
                Assert.Throws<ArgumentException>(() =>
                {
                    keyProvider.GetPublicKey(rsaKeyPair.PublicKey.Content);
                });
            }

            [Test]
            public void ShouldThrowExceptionWhenPublicKeyCannotBeConstrcuted()
            {
                keyInfoWrapper.Setup(k => k.GetPublicKeyInfo(It.IsAny<byte[]>()))
                               .Throws<ArgumentException>();
                
                Assert.Throws<CryptographicException>(() =>
                {
                    keyProvider.GetPublicKey(rsaKeyPair.PublicKey.Content);
                });
            }
            
            [Test]
            public void ShouldReturnPublicRsaKey()
            {
                IAsymmetricKey result = keyProvider.GetPublicKey(rsaKeyPair.PublicKey.Content);

                Assert.IsAssignableFrom<RsaKey>(result);
                Assert.IsFalse(result.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnPublicDsaKey()
            {
                IAsymmetricKey result = keyProvider.GetPublicKey(dsaKeyPair.PublicKey.Content);

                Assert.IsAssignableFrom<DsaKey>(result);
                Assert.IsFalse(result.IsPrivateKey);
            }
            
            [Test]
            public void ShouldReturnValidKey()
            {
                var algorithmMapper = new SignatureAlgorithmIdentifierMapper();
                var secureRandom = new SecureRandomGenerator();
                var signatureProvider = new SignatureProvider(algorithmMapper, secureRandom, new SignerUtilitiesWrapper());
                byte[] data = secureRandom.NextBytes(100);

                Signature signature = signatureProvider.CreateSignature(rsaKeyPair.PrivateKey, data);
                IAsymmetricKey result = keyProvider.GetPublicKey(rsaKeyPair.PublicKey.Content);

                Assert.IsTrue(signatureProvider.VerifySignature(result, signature));
            }
        }

        [TestFixture]
        public class GetPrivateKey : AsymmetricKeyProviderTest
        {
            [SetUp]
            public void Setup()
            {
                SetupValidKeyProvider();
                SetupValidKeyInfo();
            }

            [TestCase(CipherType.Ec)]
            [TestCase(CipherType.ElGamal)]
            [TestCase(CipherType.Pkcs5Encrypted)]
            [TestCase(CipherType.Pkcs12Encrypted)]
            [TestCase(CipherType.Unknown)]
            public void ShouldThrowExceptionWhenKeyTypeIsNotSupported(CipherType cipherType)
            {
                var typeMapperMock = new Mock<OidToCipherTypeMapper>();
                typeMapperMock.Setup(ctm => ctm.MapOidToCipherType(It.IsAny<string>()))
                              .Returns(cipherType);
                
                keyProvider = new AsymmetricKeyProvider(typeMapperMock.Object, keyInfoWrapper.Object, rsaKeyProvider, dsaKeyProvider);
                
                Assert.Throws<ArgumentException>(() =>
                {
                    keyProvider.GetPrivateKey(rsaKeyPair.PrivateKey.Content);
                });
            }

            [Test]
            public void ShouldThrowExceptionWhenKeyInfoTrowsArgumentException()
            {
                keyInfoWrapper.Setup(k => k.GetPrivateKeyInfo(It.IsAny<byte[]>()))
                               .Throws<ArgumentException>();
                
                Assert.Throws<CryptographicException>(() =>
                {
                    keyProvider.GetPrivateKey(rsaKeyPair.PrivateKey.Content);
                });
            }

            [Test]
            public void ShouldThrowExceptionWhenKeyInfoThrowsInvalidCastException()
            {
                keyInfoWrapper.Setup(k => k.GetPrivateKeyInfo(It.IsAny<byte[]>()))
                               .Throws<InvalidCastException>();
                
                Assert.Throws<CryptographicException>(() =>
                {
                    keyProvider.GetPrivateKey(rsaKeyPair.PrivateKey.Content);
                });
            }
            
            [Test]
            public void ShouldReturnPrivateRsaKey()
            {
                IAsymmetricKey result = keyProvider.GetPrivateKey(rsaKeyPair.PrivateKey.Content);

                Assert.IsAssignableFrom<RsaKey>(result);
                Assert.IsTrue(result.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnPrivateDsaKey()
            {
                IAsymmetricKey result = keyProvider.GetPrivateKey(dsaKeyPair.PrivateKey.Content);

                Assert.IsAssignableFrom<DsaKey>(result);
                Assert.IsTrue(result.IsPrivateKey);
            }
            
            [Test]
            public void ShouldReturnValidKey()
            {
                var algorithmMapper = new SignatureAlgorithmIdentifierMapper();
                var secureRandom = new SecureRandomGenerator();
                var signatureProvider = new SignatureProvider(algorithmMapper, secureRandom, new SignerUtilitiesWrapper());
                byte[] data = secureRandom.NextBytes(100);

                IAsymmetricKey result = keyProvider.GetPrivateKey(rsaKeyPair.PrivateKey.Content);
                Signature signature = signatureProvider.CreateSignature(result, data);

                Assert.IsTrue(signatureProvider.VerifySignature(rsaKeyPair.PublicKey, signature));
            }
        }

        [TestFixture]
        public class GetEncryptedPrivateKey : AsymmetricKeyProviderTest
        {
            private IAsymmetricKey encryptedPrivateKey;

            [SetUp]
            public void Setup()
            {
                keyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), rsaKeyProvider, dsaKeyProvider);
                var key = pkcsEncryptionProvider.EncryptPrivateKey(rsaKeyPair.PrivateKey, "foobar");

                encryptedPrivateKey = keyProvider.GetEncryptedPrivateKey(key.Content);
            }

            [Test]
            public void ShouldReturnEncryptedPrivateKey()
            {
                Assert.IsAssignableFrom<EncryptedKey>(encryptedPrivateKey);
                CollectionAssert.AreNotEqual(encryptedPrivateKey.Content, rsaKeyPair.PrivateKey.Content);
            }

            [Test]
            public void ShouldSetCipherType()
            {
                Assert.AreEqual(CipherType.Pkcs12Encrypted, encryptedPrivateKey.CipherType);
            }
        }
    }
}