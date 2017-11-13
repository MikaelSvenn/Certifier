using System;
using System.Security.Cryptography;
using Core.Configuration;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using Crypto.Wrappers;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class RsaKeyProviderTest
    {
        private RsaKeyProvider keyProvider;

        [OneTimeSetUp]
        public void SetupRsaKeyProviderTest()
        {
            var secureRandomGenerator = new SecureRandomGenerator();
            var asymmetricKeyGenerator = new AsymmetricKeyPairGenerator(secureRandomGenerator);

            keyProvider = new RsaKeyProvider(asymmetricKeyGenerator);
        }

        [TestFixture]
        public class CreateKeyPairTest : RsaKeyProviderTest
        {
            private IAsymmetricKeyPair keyPair;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = keyProvider.CreateKeyPair(2048);
            }

            [Test]
            public void ShouldCreatePrivateKey()
            {
                Assert.IsNotEmpty(keyPair.PrivateKey.Content);
            }

            [Test]
            public void ShouldCreatePublicKey()
            {
                Assert.IsNotEmpty(keyPair.PublicKey.Content);
            }

            [Test]
            public void ShouldSetPrivateKeyLength()
            {
                Assert.AreEqual(2048, keyPair.PrivateKey.KeySize);
            }

            [Test]
            public void ShouldSetPublicKeyLength()
            {
                Assert.AreEqual(2048, keyPair.PublicKey.KeySize);
            }

            [Test]
            public void ShouldSetPrivateKeyType()
            {
                Assert.AreEqual(AsymmetricKeyType.Private, keyPair.PrivateKey.KeyType);
            }

            [Test]
            public void ShouldSetPublicKeyType()
            {
                Assert.AreEqual(AsymmetricKeyType.Public, keyPair.PublicKey.KeyType);
            }

            [Test]
            public void ShouldCreateValidUnencryptedKeyPair()
            {
                Assert.IsTrue(keyProvider.VerifyKeyPair(keyPair));
            }

            [Test]
            public void ShouldCreateInteroperablePkcs8PrivateKey()
            {
                CngKey cngKey = CngKey.Import(keyPair.PrivateKey.Content, CngKeyBlobFormat.Pkcs8PrivateBlob);
                Assert.AreEqual("RSA", cngKey.Algorithm.Algorithm);
                Assert.AreEqual(2048, cngKey.KeySize);
            }
        }

        [TestFixture]
        public class GetKeyTest : RsaKeyProviderTest
        {
            private IAsymmetricKeyPair keyPair;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = keyProvider.CreateKeyPair(1024);
            }

            [Test]
            public void ShouldSetPublicKeyLength()
            {
                var result = keyProvider.GetKey(keyPair.PublicKey.Content, AsymmetricKeyType.Public);
                Assert.AreEqual(1024, result.KeySize);
            }

            [Test]
            public void ShouldSetPrivateKeyLength()
            {
                var result = keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Private);
                Assert.AreEqual(1024, result.KeySize);
            }

            [Test]
            public void ShouldReturnPublicRsaKey()
            {
                var result = keyProvider.GetKey(keyPair.PublicKey.Content, AsymmetricKeyType.Public);
                Assert.IsAssignableFrom<RsaKey>(result);
                Assert.IsFalse(result.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnPrivateRsaKey()
            {
                var result = keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Private);
                Assert.IsAssignableFrom<RsaKey>(result);
                Assert.IsTrue(result.IsPrivateKey);
            }

            [Test]
            public void ShouldThrowExceptionWhenWrongKeyTypeIsProvided()
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Public);
                });
            }
        }

        [TestFixture]
        public class GetPublicKeyByPrimitivesTest : RsaKeyProviderTest
        {
            private RsaKeyParameters publicKeyParameters;
            private byte[] modulus;
            private byte[] exponent;
            private IAsymmetricKey result;
            
            [OneTimeSetUp]
            public void Setup()
            {
                var keyPair = keyProvider.CreateKeyPair(1024);
                publicKeyParameters = (RsaKeyParameters) PublicKeyFactory.CreateKey(keyPair.PublicKey.Content);

                modulus = publicKeyParameters.Modulus.ToByteArray();
                exponent = publicKeyParameters.Exponent.ToByteArray();
                
                result = keyProvider.GetPublicKey(exponent, modulus);
            }

            [Test]
            public void ShouldCreateValidKey()
            {
                var keyContent = (RsaKeyParameters) PublicKeyFactory.CreateKey(result.Content);                
                Assert.AreEqual(publicKeyParameters, keyContent);
            }

            [Test]
            public void ShouldCreatePublicKey()
            {
                Assert.AreEqual(AsymmetricKeyType.Public, result.KeyType);
            }

            [Test]
            public void ShouldSetKeySize()
            {
                Assert.AreEqual(1024, result.KeySize);
            }
        }

        [TestFixture]
        public class VerifyKeyPairTest : RsaKeyProviderTest
        {
            private IAsymmetricKeyPair keyPair;
            private PkcsEncryptionProvider encryptionProvider;

            [OneTimeSetUp]
            public void SetupVerifyKeyPairTest()
            {
                keyPair = keyProvider.CreateKeyPair(1024);
            }

            [TestFixture]
            public class ShouldReturnFalseWhen : VerifyKeyPairTest
            {
                [OneTimeSetUp]
                public void Setup()
                {
                    var asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), keyProvider, null, null, null);
                    encryptionProvider = new PkcsEncryptionProvider(new PbeConfiguration(), new SecureRandomGenerator(), asymmetricKeyProvider, new PkcsEncryptionGenerator());
                }

                [Test]
                public void PublicKeyIsNotGiven()
                {
                    bool result = keyProvider.VerifyKeyPair(new AsymmetricKeyPair(keyPair.PrivateKey, null));
                    Assert.IsFalse(result);
                }

                [Test]
                public void PrivateKeyIsNotGiven()
                {
                    bool result = keyProvider.VerifyKeyPair(new AsymmetricKeyPair(null, keyPair.PublicKey));
                    Assert.IsFalse(result);
                }

                [Test]
                public void PrivateKeyIsEncrypted()
                {
                    var encryptedKey = encryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "foo");
                    bool result = keyProvider.VerifyKeyPair(new AsymmetricKeyPair(encryptedKey, keyPair.PublicKey));

                    Assert.IsFalse(result);
                }

                [Test]
                public void GivenKeysAreNotKeyPair()
                {
                    IAsymmetricKeyPair invalidKeyPair = new AsymmetricKeyPair(keyPair.PrivateKey, keyProvider.CreateKeyPair(1024).PublicKey);

                    bool result = keyProvider.VerifyKeyPair(invalidKeyPair);
                    Assert.IsFalse(result);
                }
            }

            [Test]
            public void ShouldReturnTrueWhenGivenKeysAreKeyPair()
            {
                Assert.IsTrue(keyProvider.VerifyKeyPair(keyPair));
            }
        }
    }
}