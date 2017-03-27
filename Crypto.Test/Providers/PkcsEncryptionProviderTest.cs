using System;
using System.Collections.ObjectModel;
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
    public class PkcsEncryptionProviderTest
    {
        private IConfiguration configuration;
        private Mock<SecureRandomGenerator> secureRandom;
        private Mock<IAsymmetricKeyProvider<RsaKey>> rsaKeyProvider;
        private AsymmetricKeyProvider asymmetricKeyProvider;
        private PkcsEncryptionProvider encryptionProvider;
        private Mock<PkcsEncryptionGenerator> encryptionGenerator;

        [OneTimeSetUp]
        public void SetupPkcsEncryptionProviderTest()
        {
            configuration = Mock.Of<IConfiguration>(c => c.Get<int>("SaltLengthInBytes") == 100 &&
                                                             c.Get<int>("KeyDerivationIterationCount") == 10);

            secureRandom = new Mock<SecureRandomGenerator>();
            rsaKeyProvider = new Mock<IAsymmetricKeyProvider<RsaKey>>();
            asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), rsaKeyProvider.Object);
            encryptionGenerator = new Mock<PkcsEncryptionGenerator>();
            encryptionGenerator.Setup(e => e.Encrypt(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()))
                .Returns<string, byte[], int, byte[]>((password, salt, iterationCount, content) =>
                {
                    var generator = new PkcsEncryptionGenerator();
                    return generator.Encrypt(password, salt, iterationCount, content);
                });

            encryptionProvider = new PkcsEncryptionProvider(configuration, secureRandom.Object, asymmetricKeyProvider, encryptionGenerator.Object);
        }

        [TestFixture]
        public class EncryptPrivateKey : PkcsEncryptionProviderTest
        {
            private IAsymmetricKeyPair keyPair;
            private IAsymmetricKey result;

            [OneTimeSetUp]
            public void Setup()
            {
                var secureRandomGenerator = new SecureRandomGenerator();
                var rsaProvider = new RsaKeyProvider(configuration, new RsaKeyPairGenerator(secureRandomGenerator), secureRandomGenerator);
                keyPair = rsaProvider.CreateKeyPair(1024);

                secureRandom.Setup(sr => sr.NextBytes(100))
                    .Returns(new byte[] {0x07});

                result = encryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "foo");
            }

            [Test]
            public void ShouldThrowExceptionWhenGivenKeyIsAlreadyEncrypted()
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.IsEncrypted);
                Assert.Throws<ArgumentException>(() => { encryptionProvider.EncryptPrivateKey(key, "foo"); });
            }

            [Test]
            public void ShouldGenerateSaltOfLengthDefinedByConfiguration()
            {
                secureRandom.Verify(sr => sr.NextBytes(100));
            }

            [Test]
            public void ShouldEncryptUsingGivenPassword()
            {
                encryptionGenerator.Verify(e => e.Encrypt("foo", It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()));
            }

            [Test]
            public void ShouldEncryptUsingSaltGeneratedBySecureRandom()
            {
                encryptionGenerator.Verify(e => e.Encrypt(It.IsAny<string>(), new byte[]{0x07}, It.IsAny<int>(), It.IsAny<byte[]>()));
            }

            [Test]
            public void ShouldEncryptUsingKeyIterationCountDefinedByConfiguration()
            {
                encryptionGenerator.Verify(e => e.Encrypt(It.IsAny<string>(), It.IsAny<byte[]>(), 10, It.IsAny<byte[]>()));
            }

            [Test]
            public void ShouldEncryptGivenKey()
            {
                encryptionGenerator.Verify(e => e.Encrypt(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<int>(), keyPair.PrivateKey.Content));
            }

            [Test]
            public void ShouldReturnEncryptedAsymmetricKey()
            {
                Assert.IsTrue(result.IsEncrypted);
            }

            [Test]
            public void ShouldHaveEncryptedContent()
            {
                CollectionAssert.IsNotEmpty(result.Content);
                CollectionAssert.AreNotEqual(keyPair.PrivateKey.Content, result.Content);
            }
        }

        [TestFixture]
        public class DecryptPrivateKey : PkcsEncryptionProviderTest
        {
            private IAsymmetricKeyPair keyPair;
            private IAsymmetricKey encryptedKey;
            private IAsymmetricKey result;

            [OneTimeSetUp]
            public void Setup()
            {
                var secureRandomGenerator = new SecureRandomGenerator();
                var rsaProvider = new RsaKeyProvider(configuration, new RsaKeyPairGenerator(secureRandomGenerator), secureRandomGenerator);
                keyPair = rsaProvider.CreateKeyPair(1024);

                var oidToCipherTypeMapper = new OidToCipherTypeMapper();
                encryptionProvider = new PkcsEncryptionProvider(configuration, secureRandomGenerator, new AsymmetricKeyProvider(oidToCipherTypeMapper, rsaProvider), new PkcsEncryptionGenerator());

                encryptedKey = encryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "foobar");
                result = encryptionProvider.DecryptPrivateKey(encryptedKey, "foobar");
            }

            [Test]
            public void ShouldThrowExceptionWhenIncorrectPasswordIsSupplied()
            {
                Assert.Throws<ArgumentException>(() => { encryptionProvider.DecryptPrivateKey(encryptedKey, "foobar1"); });
            }

            [Test]
            public void ShouldReturnDecryptedPrivateKey()
            {
                CollectionAssert.AreEqual(keyPair.PrivateKey.Content, result.Content);
            }
        }
    }
}