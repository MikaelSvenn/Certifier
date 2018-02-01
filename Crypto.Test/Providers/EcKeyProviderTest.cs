using System;
using System.Linq;
using System.Security.Cryptography;
using Chaos.NaCl;
using Core.Interfaces;
using Core.Model;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using NUnit.Framework;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Crypto.Test.Providers
{
    [TestFixture]
    [SingleThreaded]
    public class EcKeyProviderTest
    {
        private EcKeyProvider keyProvider;
        private IAsymmetricKeyPair keyPair;
        private AsymmetricKeyPairGenerator asymmetricKeyPairGenerator;

        [OneTimeSetUp]
        public void SetupEcKeyProviderTest()
        {
            asymmetricKeyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            keyProvider = new EcKeyProvider(asymmetricKeyPairGenerator, new FieldToCurveNameMapper());
            
            keyPair = keyProvider.CreateKeyPair("brainpoolP384t1");
        }

        [TestFixture]
        [SingleThreaded]
        public class CreateEcKeyTest : EcKeyProviderTest
        {
            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = keyProvider.CreateKeyPair("brainpoolP384t1");
            }
            
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

            [Test]
            public void ShouldCreateValidCurve25519KeyPair()
            {
                var curve25519keyPair = keyProvider.CreateKeyPair("curve25519");
                Assert.IsTrue(keyProvider.VerifyKeyPair(curve25519keyPair));
            }
            
            // Older Windows cng supports only NIST curves, while the version included in Windows 10
            // supports wider range of curves, including Brainpool suite and curve25519.
            [TestCase("P-256")]
            [TestCase("P-521")]
            public void ShouldCreateInteroperablePkcs8PrivateKey(string curveName)
            {
                var primeKeyPair = keyProvider.CreateKeyPair(curveName);
                CngKey key = CngKey.Import(primeKeyPair.PrivateKey.Content, CngKeyBlobFormat.Pkcs8PrivateBlob);
                Assert.IsTrue(key.Algorithm.Algorithm.StartsWith("ECDH"));
            }
        }
        
        [TestFixture]
        [SingleThreaded]
        public class VerifyKeyPair : EcKeyProviderTest
        {           
            [TestFixture]
            [SingleThreaded]
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
        [SingleThreaded]
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
            
            [Test]
            public void ShouldSetCurveName()
            {
                var key = keyProvider.GetKey(keyPair.PrivateKey.Content, AsymmetricKeyType.Private);
                Assert.AreEqual("brainpoolP384t1", key.Curve);
            }
        }
        
        [TestFixture]
        [SingleThreaded]
        public class GetPublicKeyByPrimitives : EcKeyProviderTest
        {
            private ECPublicKeyParameters publicKeyParameters;
            private IEcKey result;
            private byte[] q;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = keyProvider.CreateKeyPair("curve25519");
                publicKeyParameters = (ECPublicKeyParameters) PublicKeyFactory.CreateKey(keyPair.PublicKey.Content);

                q = publicKeyParameters.Q.GetEncoded();
                result = keyProvider.GetPublicKey(q, "curve25519");
            }

            [Test]
            public void ShouldCreateValidKey()
            {
                var keyContent = (ECPublicKeyParameters) PublicKeyFactory.CreateKey(result.Content);
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
                Assert.AreEqual(255, result.KeySize);
            }

            [Test]
            public void ShouldSetCurve()
            {
                Assert.AreEqual("curve25519", result.Curve);
            }
        }

        [TestFixture]
        [SingleThreaded]
        public class GetPkcs8PrivateKeyAsSec1 : EcKeyProviderTest
        {
            private IEcKey sec1Key;
            private DerSequence keySequence;
            
            [OneTimeSetUp]
            public void Setup()
            {
                sec1Key = keyProvider.GetPkcs8PrivateKeyAsSec1((IEcKey)keyPair.PrivateKey);
                keySequence = (DerSequence)Asn1Object.FromByteArray(sec1Key.Content);
            }
            
            [Test]
            public void ShouldSetSec1FormatVersion()
            {
                var formatVersion = (DerInteger) keySequence[0];
                var expectedVersion = new BigInteger("1");
                Assert.AreEqual(expectedVersion, formatVersion.Value);
            }

            [Test]
            [Repeat(100)]
            public void ShouldSetDValue()
            {
                var privateKey = (DerOctetString) keySequence[1];
                var privateKeyContent = (ECPrivateKeyParameters)PrivateKeyFactory.CreateKey(keyPair.PrivateKey.Content);
                var d = new BigInteger(privateKey.GetOctets());
                Assert.AreEqual(privateKeyContent.D, d);
            }

            [Test]
            public void ShouldSetOid()
            {
                var encodedOid = (DerTaggedObject) keySequence[2];
                DerObjectIdentifier oid = DerObjectIdentifier.GetInstance(encodedOid.GetObject());
                Assert.AreEqual("1.3.36.3.3.2.8.1.1.12", oid.Id);
            }
        }

        [TestFixture]
        [SingleThreaded]
        public class GetSec1PrivateKeyAsPkcs8 : EcKeyProviderTest
        {
            private IEcKey convertedKey;
            private DerSequence keySequence;
            
            [OneTimeSetUp]
            public void Setup()
            {
                var sec1Key = keyProvider.GetPkcs8PrivateKeyAsSec1((IEcKey)keyPair.PrivateKey);
                convertedKey = keyProvider.GetSec1PrivateKeyAsPkcs8(sec1Key.Content);
                keySequence = (DerSequence)Asn1Object.FromByteArray(convertedKey.Content);
            }

            [Test]
            public void ShouldSetPkcs8FormatVersion()
            {
                var formatVersion = (DerInteger) keySequence[0];
                var expectedVersion = new BigInteger("0");
                Assert.AreEqual(expectedVersion, formatVersion.Value);
            }

            [Test]
            public void ShouldSetIdEcPublicKeyOid()
            {
                var identifiers = (DerSequence) keySequence[1];
                var oid = (DerObjectIdentifier) identifiers[0];
                Assert.AreEqual("1.2.840.10045.2.1", oid.Id);
            }

            [Test]
            public void ShouldSetCurveOid()
            {
                var identifiers = (DerSequence) keySequence[1];
                var oid = (DerObjectIdentifier) identifiers[1];
                Assert.AreEqual("1.3.36.3.3.2.8.1.1.12", oid.Id);
            }

            [Test]
            public void ShouldReturnValidPkcs8Key()
            {
                Assert.IsTrue(keyProvider.VerifyKeyPair(new AsymmetricKeyPair(convertedKey, keyPair.PublicKey)));
            }
        }

        [TestFixture]
        [SingleThreaded]
        public class GetEd25519PublicKeyFromCurve25519 : EcKeyProviderTest
        {
            private byte[] result;
            private IAsymmetricKeyPair curve25519KeyPair;
            
            [OneTimeSetUp]
            public void Setup()
            {
                curve25519KeyPair = keyProvider.CreateKeyPair("curve25519");
                result = keyProvider.GetEd25519PublicKeyFromCurve25519(curve25519KeyPair.PrivateKey);
            }
 
            [Test]
            public void ShouldReturn32ByteResult()
            {
                Assert.AreEqual(32, result.Length);
            }

            [Test]
            [Repeat(100)]
            public void ShouldReturnEd25519PublicKey()
            {
                var privateKeyParameters = (ECPrivateKeyParameters) PrivateKeyFactory.CreateKey(curve25519KeyPair.PrivateKey.Content);
                byte[] expected = Ed25519.PublicKeyFromSeed(privateKeyParameters.D.ToByteArray());
                
                Assert.AreEqual(expected, result);
            }

            [Test]
            public void ShouldThrowWhenPublicKeyIsGiven()
            {
                Assert.Throws<InvalidOperationException>(() => keyProvider.GetEd25519PublicKeyFromCurve25519(curve25519KeyPair.PublicKey));
            }

            [Test]
            public void ShouldThrowWhenNon25519KeyIsGiven()
            {
                keyPair = keyProvider.CreateKeyPair("P-256");
                Assert.Throws<InvalidOperationException>(() => keyProvider.GetEd25519PublicKeyFromCurve25519(keyPair.PrivateKey));
            }
        }
    }
}