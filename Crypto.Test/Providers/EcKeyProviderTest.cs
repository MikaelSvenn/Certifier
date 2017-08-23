using System;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Providers;
using NUnit.Framework;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class EcKeyProviderTest
    {
        private EcKeyProvider keyProvider;
        private IAsymmetricKeyPair keyPair;
        private AsymmetricKeyPairGenerator asymmetricKeyPairGenerator;

        [OneTimeSetUp]
        public void SetupEcKeyProviderTest()
        {
            asymmetricKeyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            keyProvider = new EcKeyProvider(asymmetricKeyPairGenerator);
            
            keyPair = keyProvider.CreateKeyPair("brainpoolP384t1");
        }

        [TestFixture]
        public class CreateEcKeyTest : EcKeyProviderTest
        {
            [Test]
            public void ShouldThrowExceptionWhenKeySizeIsDefined()
            {
                Assert.Throws<InvalidOperationException>(() => { keyProvider.CreateKeyPair(2048); });
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
            public void ShouldSetPrivateKeySize()
            {
                Assert.AreEqual(384, keyPair.PrivateKey.KeySize);
            }

            [Test]
            public void ShouldSetPublicKeySize()
            {
                Assert.AreEqual(384, keyPair.PublicKey.KeySize);
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
            public void ShouldReturnPrivateEcKey()
            {
                Assert.IsAssignableFrom<EcKey>(keyPair.PrivateKey);
            }

            [Test]
            public void ShouldReturnPublicEcKey()
            {
                Assert.IsAssignableFrom<EcKey>(keyPair.PublicKey);
            }

            [Test]
            public void ShouldCreateValidUnencryptedKeyPair()
            {
                Assert.IsTrue(keyProvider.VerifyKeyPair(keyPair));
            }
        }

        [TestFixture]
        public class VerifyKeyPair : EcKeyProviderTest
        {           
            [TestFixture]
            public class ShouldReturnFalseWhen : VerifyKeyPair
            {
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
                public void KeyPairDoesNotMatch()
                {
                    IAsymmetricKeyPair alternateKeyPair = keyProvider.CreateKeyPair("brainpoolP384t1");
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(keyPair.PrivateKey, alternateKeyPair.PublicKey)));
                }

                [Test]
                public void KeysAreOfDifferentCurve()
                {
                    IAsymmetricKeyPair alternateKeyPair = keyProvider.CreateKeyPair("prime239v3");
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(keyPair.PrivateKey, alternateKeyPair.PublicKey)));
                }

                [Test]
                public void KeysAreOfDifferentType()
                {
                    var rsaKeyProvider = new RsaKeyProvider(asymmetricKeyPairGenerator);
                    var rsaKeyPair = rsaKeyProvider.CreateKeyPair(1024);
                    
                    Assert.IsFalse(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(rsaKeyPair.PrivateKey, keyPair.PublicKey)));
                }
            }

            [Test]
            public void ShouldReturnTrueWhenKeyPairIsValid()
            {
                Assert.IsTrue(keyProvider.VerifyKeyPair(keyPair));
            }
        }

        [TestFixture]
        public class GetKey : EcKeyProviderTest
        {
            [Test]
            public void ShouldSetPublicKeyLength()
            {
                var result = keyProvider.GetKey(keyPair.PublicKey.Content, AsymmetricKeyType.Public);
                Assert.AreEqual(384, result.KeySize);
            }

            [Test]
            public void ShouldSetPrivateKeyLength()
            {
                var result = keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Private);
                Assert.AreEqual(384, result.KeySize);
            }

            [Test]
            public void ShouldReturnPublicEcKey()
            {
                var result = keyProvider.GetKey(keyPair.PublicKey.Content, AsymmetricKeyType.Public);
                Assert.IsAssignableFrom<EcKey>(result);
                Assert.IsFalse(result.IsPrivateKey);
            }

            [Test]
            public void ShouldReturnPrivateEcKey()
            {
                var result = keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Private);
                Assert.IsAssignableFrom<EcKey>(result);
                Assert.IsTrue(result.IsPrivateKey);
            }

            [Test]
            public void ShouldThrowExceptionWhenKeyTypesDoNotMatch()
            {
                Assert.Throws<ArgumentException>(() => { keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Public); });
            }
        }
    }
}