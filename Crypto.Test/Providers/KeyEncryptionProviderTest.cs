using System;
using System.Collections.ObjectModel;
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
    public class KeyEncryptionProviderTest
    {
        private IConfiguration configuration;
        private Mock<SecureRandomGenerator> secureRandom;
        private Mock<IKeyProvider<RsaKey>> rsaKeyProvider;
        private AsymmetricKeyProvider asymmetricKeyProvider;
        private KeyEncryptionProvider encryptionProvider;
        private Mock<Pkcs12KeyEncryptionGenerator> pkcsEncryptionGenerator;
        private Mock<AesKeyEncryptionGenerator> aesEncryptionGenerator;

        [SetUp]
        public void SetupPkcsEncryptionProviderTest()
        {
            configuration = Mock.Of<IConfiguration>(c => c.Get<int>("SaltLengthInBytes") == 100 && 
                                                         c.Get<int>("KeyDerivationIterationCount") == 10);

            secureRandom = new Mock<SecureRandomGenerator>();
            rsaKeyProvider = new Mock<IKeyProvider<RsaKey>>();
            asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), rsaKeyProvider.Object, null, null, null);
            pkcsEncryptionGenerator = new Mock<Pkcs12KeyEncryptionGenerator>();
            pkcsEncryptionGenerator.Setup(e => e.Encrypt(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()))
                                   .Returns<string, byte[], int, byte[]>((password, salt, iterationCount, content) =>
                                   {
                                       var generator = new Pkcs12KeyEncryptionGenerator();
                                       return generator.Encrypt(password, salt, iterationCount, content);
                                   });
            
            aesEncryptionGenerator = new Mock<AesKeyEncryptionGenerator>();
            aesEncryptionGenerator.Setup(e => e.Encrypt(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()))
                                   .Returns<string, byte[], int, byte[]>((password, salt, iterationCount, content) =>
                                   {
                                       var generator = new AesKeyEncryptionGenerator();
                                       return generator.Encrypt(password, salt, iterationCount, content);
                                   });
            
            encryptionProvider = new KeyEncryptionProvider(configuration, secureRandom.Object, asymmetricKeyProvider, pkcsEncryptionGenerator.Object, aesEncryptionGenerator.Object);
        }

        [TestFixture]
        public class EncryptPrivateKey : KeyEncryptionProviderTest
        {
            private IAsymmetricKeyPair keyPair;
            private IAsymmetricKey result;

            [SetUp]
            public void SetupEncryptKey()
            {
                var secureRandomGenerator = new SecureRandomGenerator();
                var rsaProvider = new RsaKeyProvider(new AsymmetricKeyPairGenerator(secureRandomGenerator));
                keyPair = rsaProvider.CreateKeyPair(1024);

                secureRandom.Setup(sr => sr.NextBytes(100))
                    .Returns(new byte[] {0x07});
            }

            [TestFixture]
            public class WithPkcs : EncryptPrivateKey
            {
                [SetUp]
                public void Setup()
                {
                    result = encryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "foo", EncryptionType.Pkcs);
                }
                
                [Test]
                public void ShouldThrowExceptionWhenGivenKeyIsAlreadyEncrypted()
                {
                    var key = Mock.Of<IAsymmetricKey>(k => k.IsEncrypted);
                    Assert.Throws<InvalidOperationException>(() => { encryptionProvider.EncryptPrivateKey(key, "foo", EncryptionType.Pkcs); });
                }
                
                [Test]
                public void ShouldEncryptUsingKeyIterationCountDefinedByConfiguration()
                {
                    pkcsEncryptionGenerator.Verify(e => e.Encrypt(It.IsAny<string>(), It.IsAny<byte[]>(), 10, It.IsAny<byte[]>()));
                }

                [Test]
                public void ShouldNotEncryptUsingAesGenerator()
                {
                    aesEncryptionGenerator.Verify(e => e.Encrypt(It.IsAny<string>(), It.IsAny<byte[]>(), 10, It.IsAny<byte[]>()), Times.Never);
                }
                
                [Test]
                public void ShouldEncryptUsingGivenPassword()
                {
                    pkcsEncryptionGenerator.Verify(e => e.Encrypt("foo", It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()));
                }

                [Test]
                public void ShouldEncryptUsingSaltGeneratedBySecureRandom()
                {
                    pkcsEncryptionGenerator.Verify(e => e.Encrypt(It.IsAny<string>(), new byte[]{0x07}, It.IsAny<int>(), It.IsAny<byte[]>()));
                }

                [Test]
                public void ShouldEncryptGivenKey()
                {
                    pkcsEncryptionGenerator.Verify(e => e.Encrypt(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<int>(), keyPair.PrivateKey.Content));
                }
                
                [Test]
                public void ShouldThrowExceptionWhenEncryptionTypeIsNone()
                {
                    var key = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
                    Assert.Throws<InvalidOperationException>(() => encryptionProvider.EncryptPrivateKey(key, "foo", EncryptionType.None));
                }
            
                [Test]
                public void ShouldGenerateSaltOfLengthDefinedByConfiguration()
                {
                    secureRandom.Verify(sr => sr.NextBytes(100));
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
            public class WithAes : EncryptPrivateKey
            {
                [SetUp]
                public void Setup()
                {
                    result = encryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "foo", EncryptionType.Aes);
                }
                
                [Test]
                public void ShouldThrowExceptionWhenGivenKeyIsAlreadyEncrypted()
                {
                    var key = Mock.Of<IAsymmetricKey>(k => k.IsEncrypted);
                    Assert.Throws<InvalidOperationException>(() => { encryptionProvider.EncryptPrivateKey(key, "foo", EncryptionType.Aes); });
                }
                
                [Test]
                public void ShouldEncryptUsingKeyIterationCountDefinedByConfiguration()
                {
                    aesEncryptionGenerator.Verify(e => e.Encrypt(It.IsAny<string>(), It.IsAny<byte[]>(), 10, It.IsAny<byte[]>()));
                }

                [Test]
                public void ShouldNotEncryptUsingPkcsGenerator()
                {
                    pkcsEncryptionGenerator.Verify(e => e.Encrypt(It.IsAny<string>(), It.IsAny<byte[]>(), 10, It.IsAny<byte[]>()), Times.Never);
                }
                
                [Test]
                public void ShouldEncryptUsingGivenPassword()
                {
                    aesEncryptionGenerator.Verify(e => e.Encrypt("foo", It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<byte[]>()));
                }

                [Test]
                public void ShouldEncryptUsingSaltGeneratedBySecureRandom()
                {
                    aesEncryptionGenerator.Verify(e => e.Encrypt(It.IsAny<string>(), new byte[]{0x07}, It.IsAny<int>(), It.IsAny<byte[]>()));
                }

                [Test]
                public void ShouldEncryptGivenKey()
                {
                    aesEncryptionGenerator.Verify(e => e.Encrypt(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<int>(), keyPair.PrivateKey.Content));
                }
                
                [Test]
                public void ShouldThrowExceptionWhenEncryptionTypeIsNone()
                {
                    var key = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
                    Assert.Throws<InvalidOperationException>(() => encryptionProvider.EncryptPrivateKey(key, "foo", EncryptionType.None));
                }
            
                [Test]
                public void ShouldGenerateSaltOfLengthDefinedByConfiguration()
                {
                    secureRandom.Verify(sr => sr.NextBytes(100));
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
        }

        [TestFixture]
        public class DecryptPrivateKey : KeyEncryptionProviderTest
        {
            private IAsymmetricKeyPair keyPair;
            private IAsymmetricKey encryptedKey;
            private IAsymmetricKey result;

            [SetUp]
            public void SetupDecrypt()
            {
                var secureRandomGenerator = new SecureRandomGenerator();
                var rsaProvider = new RsaKeyProvider(new AsymmetricKeyPairGenerator(secureRandomGenerator));
                keyPair = rsaProvider.CreateKeyPair(1024);

                var oidToCipherTypeMapper = new OidToCipherTypeMapper();
                encryptionProvider = new KeyEncryptionProvider(configuration, secureRandomGenerator, new AsymmetricKeyProvider(oidToCipherTypeMapper, new KeyInfoWrapper(), rsaProvider, null, null, null), new Pkcs12KeyEncryptionGenerator(), new AesKeyEncryptionGenerator());
            }

            [TestFixture]
            public class PkcsEncrypted : DecryptPrivateKey
            {
                [SetUp]
                public void Setup()
                {
                    encryptedKey = encryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "foobar", EncryptionType.Pkcs);
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

            [TestFixture]
            public class AesEncrypted : DecryptPrivateKey
            {
                [SetUp]
                public void Setup()
                {
                    encryptedKey = encryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "foobar", EncryptionType.Aes);
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
}