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
using Org.BouncyCastle.Math;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class AsymmetricKeyProviderTest
    {
        private AsymmetricKeyProvider keyProvider;
        private RsaKeyProvider rsaKeyProvider;
        private DsaKeyProvider dsaKeyProvider;
        private EcKeyProvider ecKeyProvider;
        private ElGamalKeyProvider elGamalKeyProvider;
        private OidToCipherTypeMapper cipherTypeMapper;
        private PkcsEncryptionProvider pkcsEncryptionProvider;
        private Mock<KeyInfoWrapper> keyInfoWrapper;
        private IAsymmetricKeyPair rsaKeyPair;
        private IAsymmetricKeyPair dsaKeyPair;
        private IAsymmetricKeyPair ecKeyPair;
        private IAsymmetricKeyPair elGamalKeyPair;
        
        [OneTimeSetUp]
        public void SetupAsymmetricKeyProviderTest()
        {
            var configuration = Mock.Of<IConfiguration>(c => c.Get<int>("SaltLengthInBytes") == 100 &&
                                                             c.Get<int>("KeyDerivationIterationCount") == 10);
            var secureRandom = new SecureRandomGenerator();
            var asymmetricKeyPairGenerator = new AsymmetricKeyPairGenerator(secureRandom);
            rsaKeyProvider = new RsaKeyProvider(asymmetricKeyPairGenerator);
            dsaKeyProvider = new DsaKeyProvider(asymmetricKeyPairGenerator);
            ecKeyProvider = new EcKeyProvider(asymmetricKeyPairGenerator);
            elGamalKeyProvider = new ElGamalKeyProvider(asymmetricKeyPairGenerator);
            
            cipherTypeMapper = new OidToCipherTypeMapper();
            keyInfoWrapper = new Mock<KeyInfoWrapper>();
            
            SetupValidKeyInfo();
            SetupValidKeyProvider();
            
            pkcsEncryptionProvider = new PkcsEncryptionProvider(configuration, secureRandom, keyProvider, new PkcsEncryptionGenerator());
            
            rsaKeyPair = rsaKeyProvider.CreateKeyPair(2048);
            dsaKeyPair = dsaKeyProvider.CreateKeyPair(2048);
            ecKeyPair = ecKeyProvider.CreateKeyPair("secp384r1");
            
            var prime = new BigInteger("a00e283b3c624e5b2b4d9fbc2653b5185d99499b00fd1bf244c6f0bb817b4d1c451b2958d62a0f8a38caef059fb5ecd25d75ed9af403f5b5bdab97a642902f824e3c13789fed95fa106ddfe0ff4a707c85e2eb77d49e68f2808bcea18ce128b178cd287c6bc00efa9a1ad2a673fe0dceace53166f75b81d6709d5f8af7c66bb7", 16);
            var generator = new BigInteger("1db17639cdf96bc4eabba19454f0b7e5bd4e14862889a725c96eb61048dcd676ceb303d586e30f060dbafd8a571a39c4d823982117da5cc4e0f89c77388b7a08896362429b94a18a327604eb7ff227bffbc83459ade299e57b5f77b50fb045250934938efa145511166e3197373e1b5b1e52de713eb49792bedde722c6717abf", 16);
            elGamalKeyPair = elGamalKeyProvider.CreateKeyPair(1024, prime, generator);
        }

        private void SetupValidKeyProvider()
        {
            keyProvider = new AsymmetricKeyProvider(cipherTypeMapper, keyInfoWrapper.Object, rsaKeyProvider, dsaKeyProvider, ecKeyProvider, elGamalKeyProvider);
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
            
            [TestCase(CipherType.Pkcs5Encrypted)]
            [TestCase(CipherType.Pkcs12Encrypted)]
            [TestCase(CipherType.Unknown)]
            public void ShouldThrowExceptionWhenKeyTypeIsNotSupported(CipherType cipherType)
            {
                var typeMapperMock = new Mock<OidToCipherTypeMapper>();
                typeMapperMock.Setup(ctm => ctm.MapOidToCipherType(It.IsAny<string>()))
                                .Returns(cipherType);

                keyProvider = new AsymmetricKeyProvider(typeMapperMock.Object, keyInfoWrapper.Object, rsaKeyProvider, dsaKeyProvider, ecKeyProvider, elGamalKeyProvider);
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
            public void ShouldReturnPublicEcKey()
            {
                IAsymmetricKey result = keyProvider.GetPublicKey(ecKeyPair.PublicKey.Content);

                Assert.IsAssignableFrom<EcKey>(result);
                Assert.IsFalse(result.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnPublicElGamalKey()
            {
                IAsymmetricKey result = keyProvider.GetPublicKey(elGamalKeyPair.PublicKey.Content);

                Assert.IsAssignableFrom<ElGamalKey>(result);
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

            [TestCase(CipherType.Pkcs5Encrypted)]
            [TestCase(CipherType.Pkcs12Encrypted)]
            [TestCase(CipherType.Unknown)]
            public void ShouldThrowExceptionWhenKeyTypeIsNotSupported(CipherType cipherType)
            {
                var typeMapperMock = new Mock<OidToCipherTypeMapper>();
                typeMapperMock.Setup(ctm => ctm.MapOidToCipherType(It.IsAny<string>()))
                              .Returns(cipherType);
                
                keyProvider = new AsymmetricKeyProvider(typeMapperMock.Object, keyInfoWrapper.Object, rsaKeyProvider, dsaKeyProvider, ecKeyProvider, elGamalKeyProvider);
                
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
            public void ShouldReturnPrivateEcKey()
            {
                IAsymmetricKey result = keyProvider.GetPrivateKey(ecKeyPair.PrivateKey.Content);

                Assert.IsAssignableFrom<EcKey>(result);
                Assert.IsTrue(result.IsPrivateKey);
            }
            
            [Test]
            public void ShouldReturnPrivateElGamalKey()
            {
                IAsymmetricKey result = keyProvider.GetPrivateKey(elGamalKeyPair.PrivateKey.Content);

                Assert.IsAssignableFrom<ElGamalKey>(result);
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
                keyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), rsaKeyProvider, dsaKeyProvider, ecKeyProvider, elGamalKeyProvider);
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