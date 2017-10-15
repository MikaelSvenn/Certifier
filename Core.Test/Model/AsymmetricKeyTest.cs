using System.Collections;
using Core.Interfaces;
using Core.Model;
using NUnit.Framework;

namespace Core.Test.Model
{
    [TestFixture]
    public class AsymmetricKeyTest
    {
        public class AsymmetricKeyTestCaseData
        {
            public static IEnumerable IsEncryptedShouldBeFalse
            {
                get
                {
                    yield return new TestCaseData(new RsaKey(null, AsymmetricKeyType.Private, 1)).SetName("Rsa private key")
                                                                                                 .Returns(false);
                    
                    yield return new TestCaseData(new RsaKey(null, AsymmetricKeyType.Public, 1)).SetName("Rsa public key")
                                                                                                 .Returns(false);
                    
                    yield return new TestCaseData(new DsaKey(null, AsymmetricKeyType.Private, 1)).SetName("Dsa private key")
                                                                                                 .Returns(false);
                    
                    yield return new TestCaseData(new DsaKey(null, AsymmetricKeyType.Public, 1)).SetName("Dsa public key")
                                                                                                .Returns(false);
                    
                    yield return new TestCaseData(new EcKey(null, AsymmetricKeyType.Private, 1)).SetName("EC private key")
                                                                                                 .Returns(false);
                    
                    yield return new TestCaseData(new EcKey(null, AsymmetricKeyType.Public, 1)).SetName("EC public key")
                                                                                                .Returns(false);
                    
                    yield return new TestCaseData(new ElGamalKey(null, AsymmetricKeyType.Private, 1)).SetName("ElGamal private key")
                                                                                                 .Returns(false);
                    
                    yield return new TestCaseData(new ElGamalKey(null, AsymmetricKeyType.Public, 1)).SetName("ElGamal public key")
                                                                                                .Returns(false);
                }
            }
            
            public static IEnumerable IsEncryptedShouldBeTrue
            {
                get
                {
                    yield return new TestCaseData(new RsaKey(null, AsymmetricKeyType.Encrypted, 1)).SetName("Rsa encrypted key")
                                                                                                 .Returns(true);
                    
                    yield return new TestCaseData(new DsaKey(null, AsymmetricKeyType.Encrypted, 1)).SetName("Dsa encrypted key")
                                                                                                   .Returns(true);
                    
                    yield return new TestCaseData(new EcKey(null, AsymmetricKeyType.Encrypted, 1)).SetName("EC encrypted key")
                                                                                                   .Returns(true);
                    
                    yield return new TestCaseData(new ElGamalKey(null, AsymmetricKeyType.Encrypted, 1)).SetName("ElGamal encrypted key")
                                                                                                   .Returns(true);
                }
            }
            
            public static IEnumerable IsPrivateShouldBeFalse
            {
                get
                {
                    yield return new TestCaseData(new RsaKey(null, AsymmetricKeyType.Encrypted, 1)).SetName("Rsa encrypted key")
                                                                                                   .Returns(false);
                    
                    yield return new TestCaseData(new RsaKey(null, AsymmetricKeyType.Public, 1)).SetName("Rsa public key")
                                                                                                   .Returns(false);
                    
                    yield return new TestCaseData(new DsaKey(null, AsymmetricKeyType.Encrypted, 1)).SetName("Dsa encrypted key")
                                                                                                   .Returns(false);
                    
                    yield return new TestCaseData(new DsaKey(null, AsymmetricKeyType.Public, 1)).SetName("Dsa public key")
                                                                                                .Returns(false);
                    
                    yield return new TestCaseData(new EcKey(null, AsymmetricKeyType.Encrypted, 1)).SetName("EC encrypted key")
                                                                                                   .Returns(false);
                    
                    yield return new TestCaseData(new EcKey(null, AsymmetricKeyType.Public, 1)).SetName("EC public key")
                                                                                                .Returns(false);
                    
                    yield return new TestCaseData(new ElGamalKey(null, AsymmetricKeyType.Encrypted, 1)).SetName("ElGamal encrypted key")
                                                                                                   .Returns(false);
                    
                    yield return new TestCaseData(new ElGamalKey(null, AsymmetricKeyType.Public, 1)).SetName("ElGamal public key")
                                                                                                .Returns(false);
                }
            }
            
            public static IEnumerable IsPrivateShouldBeTrue
            {
                get
                {
                    yield return new TestCaseData(new RsaKey(null, AsymmetricKeyType.Private, 1)).SetName("Rsa private key")
                                                                                                   .Returns(true);
                    
                    yield return new TestCaseData(new DsaKey(null, AsymmetricKeyType.Private, 1)).SetName("Dsa private key")
                                                                                                 .Returns(true);
                    
                    yield return new TestCaseData(new EcKey(null, AsymmetricKeyType.Private, 1)).SetName("EC private key")
                                                                                                 .Returns(true);
                    
                    yield return new TestCaseData(new ElGamalKey(null, AsymmetricKeyType.Private, 1)).SetName("ElGamal private key")
                                                                                                 .Returns(true);
                }
            }
            
            public static IEnumerable CipherTypeShouldMatchKey
            {
                get
                {
                    yield return new TestCaseData(new RsaKey(null, AsymmetricKeyType.Encrypted, 1)).SetName("Rsa encrypted key")
                                                                                                   .Returns(Core.Model.CipherType.Rsa);
                    
                    yield return new TestCaseData(new RsaKey(null, AsymmetricKeyType.Private, 1)).SetName("Rsa private key")
                                                                                                   .Returns(Core.Model.CipherType.Rsa);
                    
                    yield return new TestCaseData(new RsaKey(null, AsymmetricKeyType.Public, 1)).SetName("Rsa public key")
                                                                                                 .Returns(Core.Model.CipherType.Rsa);
                    
                    yield return new TestCaseData(new DsaKey(null, AsymmetricKeyType.Encrypted, 1)).SetName("Dsa encrypted key")
                                                                                                   .Returns(Core.Model.CipherType.Dsa);
                    
                    yield return new TestCaseData(new DsaKey(null, AsymmetricKeyType.Private, 1)).SetName("Dsa private key")
                                                                                                 .Returns(Core.Model.CipherType.Dsa);
                    
                    yield return new TestCaseData(new DsaKey(null, AsymmetricKeyType.Public, 1)).SetName("Dsa public key")
                                                                                                .Returns(Core.Model.CipherType.Dsa);
                    
                    yield return new TestCaseData(new EcKey(null, AsymmetricKeyType.Encrypted, 1)).SetName("EC encrypted key")
                                                                                                   .Returns(Core.Model.CipherType.Ec);
                    
                    yield return new TestCaseData(new EcKey(null, AsymmetricKeyType.Private, 1)).SetName("EC private key")
                                                                                                 .Returns(Core.Model.CipherType.Ec);
                    
                    yield return new TestCaseData(new EcKey(null, AsymmetricKeyType.Public, 1)).SetName("EC public key")
                                                                                                .Returns(Core.Model.CipherType.Ec);
                    
                    yield return new TestCaseData(new ElGamalKey(null, AsymmetricKeyType.Encrypted, 1)).SetName("ElGamal encrypted key")
                                                                                                   .Returns(Core.Model.CipherType.ElGamal);
                    
                    yield return new TestCaseData(new ElGamalKey(null, AsymmetricKeyType.Private, 1)).SetName("ElGamal private key")
                                                                                                 .Returns(Core.Model.CipherType.ElGamal);
                    
                    yield return new TestCaseData(new ElGamalKey(null, AsymmetricKeyType.Public, 1)).SetName("ElGamal public key")
                                                                                                .Returns(Core.Model.CipherType.ElGamal);
                }
            }
        }
        
        [TestFixture]
        public class IsEncrypted : AsymmetricKeyTest
        {
            [Test, TestCaseSource(typeof(AsymmetricKeyTestCaseData), nameof(AsymmetricKeyTestCaseData.IsEncryptedShouldBeFalse))]
            public bool ShouldRetrunFalseWhenKeyTypeIsNotEncrypted(IAsymmetricKey key) => key.IsEncrypted;

            [Test, TestCaseSource(typeof(AsymmetricKeyTestCaseData), nameof(AsymmetricKeyTestCaseData.IsEncryptedShouldBeTrue))]
            public bool ShouldReturnTrueWhenKeyTypeIsEncrypted(IAsymmetricKey key) => key.IsEncrypted;
        }

        [TestFixture]
        public class IsPrivateKey : AsymmetricKeyTest
        {
            [Test, TestCaseSource(typeof(AsymmetricKeyTestCaseData), nameof(AsymmetricKeyTestCaseData.IsPrivateShouldBeFalse))]
            public bool ShouldReturnFalseWhenKeyTypeIsPublic(IAsymmetricKey key) => key.IsPrivateKey;

            [Test, TestCaseSource(typeof(AsymmetricKeyTestCaseData), nameof(AsymmetricKeyTestCaseData.IsPrivateShouldBeTrue))]
            public bool ShouldReturnTrueWhenKeyTypeIsPrivate(IAsymmetricKey key) => key.IsPrivateKey;
        }
        
        [TestFixture]
        public class CipherType : AsymmetricKeyTest
        {
            [Test, TestCaseSource(typeof(AsymmetricKeyTestCaseData), nameof(AsymmetricKeyTestCaseData.CipherTypeShouldMatchKey))]
            public Core.Model.CipherType ShouldHaveCipherType(IAsymmetricKey key) => key.CipherType;
        }
    }
}