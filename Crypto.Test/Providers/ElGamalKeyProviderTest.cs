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
            keyProvider = new ElGamalKeyProvider(keyGenerator);

            //Creating Elgamal key parameters is very slow; these pre-generated values are from BC Elgamal test suite.
            var prime = new BigInteger("a00e283b3c624e5b2b4d9fbc2653b5185d99499b00fd1bf244c6f0bb817b4d1c451b2958d62a0f8a38caef059fb5ecd25d75ed9af403f5b5bdab97a642902f824e3c13789fed95fa106ddfe0ff4a707c85e2eb77d49e68f2808bcea18ce128b178cd287c6bc00efa9a1ad2a673fe0dceace53166f75b81d6709d5f8af7c66bb7", 16);
            var generator = new BigInteger("1db17639cdf96bc4eabba19454f0b7e5bd4e14862889a725c96eb61048dcd676ceb303d586e30f060dbafd8a571a39c4d823982117da5cc4e0f89c77388b7a08896362429b94a18a327604eb7ff227bffbc83459ade299e57b5f77b50fb045250934938efa145511166e3197373e1b5b1e52de713eb49792bedde722c6717abf", 16);

            keyPair = keyProvider.CreateKeyPair(1024, prime, generator);
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
                Assert.AreEqual(1024, keyPair.PrivateKey.KeySize);
            }

            [Test]
            public void ShouldSetPublicKeySize()
            {
                Assert.AreEqual(1024, keyPair.PublicKey.KeySize);
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
                private PkcsEncryptionProvider encryptionProvider;
                
                [OneTimeSetUp]
                public void Setup()
                {
                    var asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), null, null, null, null);
                    encryptionProvider = new PkcsEncryptionProvider(new PbeConfiguration(), new SecureRandomGenerator(), asymmetricKeyProvider, new PkcsEncryptionGenerator());
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
                Assert.AreEqual(1024, result.KeySize);
            }

            [Test]
            public void ShouldSetPrivateKeyLength()
            {
                var result = keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Private);
                Assert.AreEqual(1024, result.KeySize);
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