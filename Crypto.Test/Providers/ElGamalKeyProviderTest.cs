using System;
using Core.Configuration;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using Crypto.Wrappers;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class ElGamalKeyProviderTest
    {
        private ElGamalKeyProvider keyProvider;
        private IAsymmetricKeyPair keyPair;
        
        [OneTimeSetUp]
        public void SetupElGamalKeyProviderTest()
        {
            var secureRandomGenerator = new SecureRandomGenerator();
            var keyGenerator = new AsymmetricKeyPairGenerator(secureRandomGenerator);
            var primeMapper = new Rfc3526PrimeMapper();
            keyProvider = new ElGamalKeyProvider(keyGenerator, primeMapper);

            keyPair = keyProvider.CreateKeyPair(2048, true);
        }

        [TestFixture]
        public class CreateElGamalKeyTest : ElGamalKeyProviderTest
        {
            [Test]
            public void ShouldCreatePrivateKey()
            {
                Assert.IsNotEmpty(keyPair.PrivateKey.Content);
                Assert.IsTrue(keyPair.PrivateKey.IsPrivateKey);
            }

            [Test]
            public void ShouldCreatePublicKey()
            {
                Assert.IsNotEmpty(keyPair.PublicKey.Content);
                Assert.IsFalse(keyPair.PublicKey.IsPrivateKey);
            }

            [Test]
            public void ShouldSetPrivateKeySize()
            {
                Assert.AreEqual(2048, keyPair.PrivateKey.KeySize);
            }

            [Test]
            public void ShouldSetPublicKeySize()
            {
                Assert.AreEqual(2048, keyPair.PublicKey.KeySize);
            }

            [Test]
            public void ShouldRetrunElGamalPrivateKey()
            {
                Assert.IsAssignableFrom<ElGamalKey>(keyPair.PrivateKey);
            }

            [Test]
            public void ShouldReturnElGamalPublicKey()
            {
                Assert.IsAssignableFrom<ElGamalKey>(keyPair.PublicKey);
            }

            [Test]
            public void ShouldCreateValidUnencryptedKeyPair()
            {
                Assert.IsTrue(keyProvider.VerifyKeyPair(keyPair));
            }
        }

        [TestFixture]
        public class VerifyKeyPairTest : ElGamalKeyProviderTest
        {
            [TestFixture]
            public class ShouldReturnFalseWhen : VerifyKeyPairTest
            {
                private Pkcs8EncryptionProvider encryptionProvider;
                
                [OneTimeSetUp]
                public void Setup()
                {
                    var asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), null, null, null, null);
                    encryptionProvider = new Pkcs8EncryptionProvider(new PbeConfiguration(), new SecureRandomGenerator(), asymmetricKeyProvider, new Pkcs12EncryptionGenerator());
                }
                
                [Test]
                public void PrivateKeyIsNotGiven()
                {
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(null, keyPair.PublicKey)));
                }

                [Test]
                public void PublicKeyIsNotGiven()
                {
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(keyPair.PrivateKey, null)));
                }

                [Test]
                public void PrivateKeyIsEncrypted()
                {
                    IAsymmetricKey encryptedPrivateKey = encryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "foo");
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(encryptedPrivateKey, keyPair.PublicKey)));
                }

                [Test]
                public void PrivateKeyIsNotValid()
                {
                    ElGamalPrivateKeyParameters modifiedPrivateKey = (ElGamalPrivateKeyParameters) PrivateKeyFactory.CreateKey(keyPair.PrivateKey.Content);
                    IAsymmetricKey privateKey = GetModifiedPrivateKey(modifiedPrivateKey.X.Add(BigInteger.One), modifiedPrivateKey.Parameters);
                    
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, keyPair.PublicKey)));
                }

                private IAsymmetricKey GetModifiedPrivateKey(BigInteger modifiedX, ElGamalParameters parameters)
                {
                    var modifiedParameters = new ElGamalPrivateKeyParameters(modifiedX, parameters);
                    byte[] privateKeyContent = PrivateKeyInfoFactory.CreatePrivateKeyInfo(modifiedParameters)
                                                                    .ToAsn1Object()
                                                                    .GetDerEncoded();
    
                    var privateKey = new ElGamalKey(privateKeyContent, AsymmetricKeyType.Private, modifiedParameters.Parameters.P.BitLength);
                    return privateKey;
                }

                private IAsymmetricKey GetModifiedPublicKey(BigInteger modifiedY, ElGamalParameters parameters)
                {
                    var modifiedParameters = new ElGamalPublicKeyParameters(modifiedY, parameters);
                    byte[] publicKeyContent = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(modifiedParameters)
                                                                    .ToAsn1Object()
                                                                    .GetDerEncoded();
    
                    var publicKey = new ElGamalKey(publicKeyContent, AsymmetricKeyType.Public, modifiedParameters.Parameters.P.BitLength);
                    return publicKey;
                }
                
                [Test]
                public void PublicKeyIsNotValid()
                {
                    ElGamalPublicKeyParameters keyParameters = (ElGamalPublicKeyParameters) PublicKeyFactory.CreateKey(keyPair.PublicKey.Content);
                    IAsymmetricKey publicKey = GetModifiedPublicKey(keyParameters.Y.Add(BigInteger.One), keyParameters.Parameters);
                    
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(keyPair.PrivateKey, publicKey)));
                }
            }

            [TestFixture]
            public class ShouldReturnTrueWhen : VerifyKeyPairTest
            {
                [Test]
                public void KeyPairIsValid()
                {
                    Assert.IsTrue(keyProvider.VerifyKeyPair(keyPair));
                }
            }
        }

        [TestFixture]
        public class GetKeyTest : ElGamalKeyProviderTest
        {
            [Test]
            public void ShouldSetPublicKeyLength()
            {
                var result = keyProvider.GetKey(keyPair.PublicKey.Content, AsymmetricKeyType.Public);
                Assert.AreEqual(2048, result.KeySize);
            }

            [Test]
            public void ShouldSetPrivateKeyLength()
            {
                var result = keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Private);
                Assert.AreEqual(2048, result.KeySize);
            }

            [Test]
            public void ShouldReturnPublicDsaKey()
            {
                var result = keyProvider.GetKey(keyPair.PublicKey.Content, AsymmetricKeyType.Public);
                Assert.IsAssignableFrom<ElGamalKey>(result);
                Assert.IsFalse(result.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnPrivateDsaKey()
            {
                var result = keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Private);
                Assert.IsAssignableFrom<ElGamalKey>(result);
                Assert.IsTrue(result.IsPrivateKey);
            }

            [Test]
            public void ShouldThrowExceptionWhenKeyTypeDoesNotMatch()
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Public);
                });
            }
        }
    }
}