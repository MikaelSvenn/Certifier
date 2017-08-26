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
    public class DsaKeyProviderTest
    {
        private DsaKeyProvider keyProvider;
        private IAsymmetricKeyPair keyPair;
        
        [OneTimeSetUp]
        public void SetupDsaKeyProviderTest()
        {
            var secureRandomGenerator = new SecureRandomGenerator();
            var keyGenerator = new AsymmetricKeyPairGenerator(secureRandomGenerator);

            keyProvider = new DsaKeyProvider(keyGenerator);
            keyPair = keyProvider.CreateKeyPair(2048);
        }

        [TestFixture]
        public class CreateDsaKeyTest : DsaKeyProviderTest
        {           
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
            public void ShouldMarkPrivateKeyAsPrivate()
            {
                Assert.IsTrue(keyPair.PrivateKey.IsPrivateKey);
            }

            [Test]
            public void ShouldNotMarkPublicKeyAsPrivate()
            {
                Assert.IsFalse(keyPair.PublicKey.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnPrivateDsaKey()
            {
                Assert.IsAssignableFrom<DsaKey>(keyPair.PrivateKey);
            }

            [Test]
            public void ShouldReturnPublicDsaKey()
            {
                Assert.IsAssignableFrom<DsaKey>(keyPair.PublicKey);
            }

            [Test]
            public void ShouldCreateValidUnencryptedKeyPair()
            {
                Assert.IsTrue(keyProvider.VerifyKeyPair(keyPair));
            }
        }

        [TestFixture]
        public class VerifyKeyPair : DsaKeyProviderTest
        {
            private PkcsEncryptionProvider encryptionProvider;
            
            [TestFixture]
            public class ShouldReturnFalseWhen : VerifyKeyPair
            {
                [OneTimeSetUp]
                public void Setup()
                {
                    var asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), null, null, null);
                    encryptionProvider = new PkcsEncryptionProvider(new PbeConfiguration(), new SecureRandomGenerator(), asymmetricKeyProvider, new PkcsEncryptionGenerator());
                }
                
                [Test]
                public void WhenPrivateKeyIsNotGiven()
                {
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(null, keyPair.PublicKey)));
                }

                [Test]
                public void WhenPublicKeyIsNotGiven()
                {
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(keyPair.PrivateKey, null)));
                }

                [Test]
                public void WhenPrivateKeyIsEncrypted()
                {
                    IAsymmetricKey encryptedPrivateKey = encryptionProvider.EncryptPrivateKey(keyPair.PrivateKey, "foo");
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(encryptedPrivateKey, keyPair.PublicKey)));
                }
                
                [Test]
                public void PrivateKeyParameterIsEmpty()
                {
                    DsaPrivateKeyParameters modifiedPrivateKey = (DsaPrivateKeyParameters) PrivateKeyFactory.CreateKey(keyPair.PrivateKey.Content);
                    var privateKey = GetModifiedPrivateKey(BigInteger.Zero, modifiedPrivateKey.Parameters);
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, keyPair.PublicKey)));
                }

                private IAsymmetricKey GetModifiedPrivateKey(BigInteger X, DsaParameters domainParameters)
                {
                    var modifiedPrivateKey = new DsaPrivateKeyParameters(X, domainParameters);
                    byte[] privateKeyContent = PrivateKeyInfoFactory.CreatePrivateKeyInfo(modifiedPrivateKey)
                                                                    .ToAsn1Object()
                                                                    .GetDerEncoded();

                    var privateKey = new DsaKey(privateKeyContent, AsymmetricKeyType.Private, modifiedPrivateKey.Parameters.P.BitLength);
                    return privateKey;
                }

                [Test]
                public void PrivateKeyIsNotValid()
                {
                    DsaPrivateKeyParameters modifiedPrivateKey = (DsaPrivateKeyParameters) PrivateKeyFactory.CreateKey(keyPair.PrivateKey.Content);
                    var privateKey = GetModifiedPrivateKey(modifiedPrivateKey.X.Add(BigInteger.One), modifiedPrivateKey.Parameters);
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, keyPair.PublicKey)));
                }

                [Test]
                public void PublicKeyIsEmpty()
                {
                    DsaPublicKeyParameters keyParameters = (DsaPublicKeyParameters) PublicKeyFactory.CreateKey(keyPair.PublicKey.Content);
                    DsaKey publicKey = GetModifiedPublicKey(BigInteger.Zero, keyParameters.Parameters.G, keyParameters.Parameters.P, keyParameters.Parameters.Q);
                    
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(keyPair.PrivateKey, publicKey)));
                }
                
                [Test]
                public void PublicKeyIsNotValid()
                {
                    DsaPublicKeyParameters keyParameters = (DsaPublicKeyParameters) PublicKeyFactory.CreateKey(keyPair.PublicKey.Content);
                    DsaKey publicKey = GetModifiedPublicKey(keyParameters.Y.Add(BigInteger.One), keyParameters.Parameters.G, keyParameters.Parameters.P, keyParameters.Parameters.Q);
                    
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(keyPair.PrivateKey, publicKey)));
                }
                
                [Test]
                public void DomainParameterGAreNotEqual()
                {
                    DsaPublicKeyParameters keyParameters = (DsaPublicKeyParameters) PublicKeyFactory.CreateKey(keyPair.PublicKey.Content);
                    DsaKey publicKey = GetModifiedPublicKey(keyParameters.Y, keyParameters.Parameters.G.Add(BigInteger.One), keyParameters.Parameters.P, keyParameters.Parameters.Q);
                    
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(keyPair.PrivateKey, publicKey)));
                }

                private DsaKey GetModifiedPublicKey(BigInteger Y, BigInteger G, BigInteger P, BigInteger Q)
                {
                    var modifiedParameters = new DsaParameters(P, Q, G);

                    var keyParameters = new DsaPublicKeyParameters(Y, modifiedParameters);
                    byte[] publicKeyContent = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyParameters)
                                                                         .ToAsn1Object()
                                                                         .GetDerEncoded();

                    var publicKey = new DsaKey(publicKeyContent, AsymmetricKeyType.Public, keyParameters.Parameters.P.BitLength);
                    return publicKey;
                }

                [Test]
                public void DomainParameterQAreNotEqual()
                {
                    DsaPublicKeyParameters keyParameters = (DsaPublicKeyParameters) PublicKeyFactory.CreateKey(keyPair.PublicKey.Content);
                    DsaKey publicKey = GetModifiedPublicKey(keyParameters.Y, keyParameters.Parameters.G, keyParameters.Parameters.P, keyParameters.Parameters.Q.Add(BigInteger.One));
                    
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(keyPair.PrivateKey, publicKey)));
                }

                [Test]
                public void DomainParameterPAreNotEqual()
                {
                    DsaPublicKeyParameters keyParameters = (DsaPublicKeyParameters) PublicKeyFactory.CreateKey(keyPair.PublicKey.Content);
                    DsaKey publicKey = GetModifiedPublicKey(keyParameters.Y, keyParameters.Parameters.G, keyParameters.Parameters.P.Add(BigInteger.One), keyParameters.Parameters.Q);
                    
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(keyPair.PrivateKey, publicKey)));
                }
            }
            
            [Test]
            public void ShouldReturnTrueWhenKeyPairIsValid()
            {
                Assert.IsTrue(keyProvider.VerifyKeyPair(keyPair));
            }
        }

        [TestFixture]
        public class GetKey : DsaKeyProviderTest
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
                Assert.IsAssignableFrom<DsaKey>(result);
                Assert.IsFalse(result.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnPrivateDsaKey()
            {
                var result = keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Private);
                Assert.IsAssignableFrom<DsaKey>(result);
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