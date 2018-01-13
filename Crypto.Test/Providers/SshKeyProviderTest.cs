using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core.Interfaces;
using Core.SystemWrappers;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using Moq;
using NUnit.Framework;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class SshKeyProviderTest
    {
        private SshKeyProvider provider;
        private EcKeyProvider ecKeyProvider;
        private AsymmetricKeyPairGenerator keyPairGenerator;
        private Base64Wrapper base64;

        [SetUp]
        public void SetupSshKeyProviderTest()
        {
            keyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            ecKeyProvider = new EcKeyProvider(keyPairGenerator, new FieldToCurveNameMapper());
            provider = new SshKeyProvider(new EncodingWrapper(), new Base64Wrapper(), Mock.Of<IRsaKeyProvider>(), Mock.Of<IDsaKeyProvider>(), ecKeyProvider);
            
            base64 = new Base64Wrapper();
        }

        [TestFixture]
        public class IsSupportedCurve : SshKeyProviderTest
        {
            [TestCase("curve25519")]
            [TestCase("P-256")]
            [TestCase("secp256r1")]
            [TestCase("prime256v1")]
            [TestCase("P-384")]
            [TestCase("secp384r1")]
            [TestCase("P-521")]
            [TestCase("secp521r1")]
            public void ShouldReturnTrueForSshSupportedCurve(string curve)
            {
                Assert.IsTrue(provider.IsSupportedCurve(curve));
            }

            [TestCase("brainpoolP384t1")]
            [TestCase("prime239v3")]
            [TestCase("c2tnb191v3")]
            [TestCase("FRP256v1")]
            public void ShouldReturnFalseForNotSupportedCurve(string curve)
            {
                Assert.IsFalse(provider.IsSupportedCurve(curve));
            }
        }

        [TestFixture]
        public class GetCurveSshHeader : SshKeyProviderTest
        {
            [TestCase("curve25519", ExpectedResult = "ssh-ed25519")]
            [TestCase("P-256", ExpectedResult = "ecdsa-sha2-nistp256")]
            [TestCase("secp256r1", ExpectedResult = "ecdsa-sha2-nistp256")]
            [TestCase("prime256v1", ExpectedResult = "ecdsa-sha2-nistp256")]
            [TestCase("P-384", ExpectedResult = "ecdsa-sha2-nistp384")]
            [TestCase("secp384r1", ExpectedResult = "ecdsa-sha2-nistp384")]
            [TestCase("P-521", ExpectedResult = "ecdsa-sha2-nistp521")]
            [TestCase("secp521r1", ExpectedResult = "ecdsa-sha2-nistp521")]
            public string ShouldReturnSshHeaderForGivenCurve(string curve) => provider.GetCurveSshHeader(curve);
        }

        private static byte[] ReadNextContent(MemoryStream stream)
        {
            var contentLengthBytes = new byte[4];
            stream.Read(contentLengthBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(contentLengthBytes);
            }

            int headerLength = BitConverter.ToInt32(contentLengthBytes, 0);
            int contentLength = headerLength;
            
            var contentBytes = new byte[contentLength];
            stream.Read(contentBytes, 0, contentLength);
            return contentBytes;
        }
        
        [TestFixture]
        public class GetRsaPublicKeyContent : SshKeyProviderTest
        {
            private IAsymmetricKey key;
            private byte[] result;

            private byte[] rawHeader;
            private byte[] rawExponent;
            private byte[] rawModulus;
            
            [SetUp]
            public void Setup()
            {
                var rsaKeyProvider = new RsaKeyProvider(keyPairGenerator);
                IAsymmetricKeyPair keyPair = rsaKeyProvider.CreateKeyPair(2048);
                key = keyPair.PublicKey;

                string keyContent = provider.GetRsaPublicKeyContent(key);
                result = base64.FromBase64String(keyContent);
                
                using (var stream = new MemoryStream(result))
                {
                    rawHeader = ReadNextContent(stream);
                    rawExponent = ReadNextContent(stream);
                    rawModulus = ReadNextContent(stream);
                }
            }

            [Test]
            public void ShouldSetHeader()
            {               
                string headerContent = Encoding.UTF8.GetString(rawHeader);
                Assert.AreEqual("ssh-rsa", headerContent);
            }

            [Test]
            public void ShouldSetExponent()
            {
                var exponent = new BigInteger(rawExponent);
                var keyParameters = (RsaKeyParameters)PublicKeyFactory.CreateKey(key.Content);

                Assert.AreEqual(keyParameters.Exponent, exponent);
            }

            [Test]
            public void ShouldSetModulus()
            {
                var modulus = new BigInteger(rawModulus);
                var keyParameters = (RsaKeyParameters)PublicKeyFactory.CreateKey(key.Content);

                Assert.AreEqual(keyParameters.Modulus, modulus);
            }
        }

        [TestFixture]
        public class GetDsaPublicKeyContent : SshKeyProviderTest
        {
            private IAsymmetricKey key;
            private byte[] result;

            private byte[] rawHeader;
            private byte[] rawP;
            private byte[] rawQ;
            private byte[] rawG;
            private byte[] rawY;
            
            [SetUp]
            public void Setup()
            {
                var dsaKeyProvider = new DsaKeyProvider(keyPairGenerator);
                IAsymmetricKeyPair keyPair = dsaKeyProvider.CreateKeyPair(2048);
                key = keyPair.PublicKey;

                string keyContent = provider.GetDsaPublicKeyContent(key);
                result = base64.FromBase64String(keyContent);
                
                using (var stream = new MemoryStream(result))
                {
                    rawHeader = ReadNextContent(stream);
                    rawP = ReadNextContent(stream);
                    rawQ = ReadNextContent(stream);
                    rawG = ReadNextContent(stream);
                    rawY = ReadNextContent(stream);
                }
            }
            
            [Test]
            public void ShouldSetHeader()
            {               
                string headerContent = Encoding.UTF8.GetString(rawHeader);
                Assert.AreEqual("ssh-dss", headerContent);
            }

            [Test]
            public void ShouldSetP()
            {
                var p = new BigInteger(rawP);
                var keyParameters = (DsaPublicKeyParameters)PublicKeyFactory.CreateKey(key.Content);
                
                Assert.AreEqual(keyParameters.Parameters.P, p);
            }
            
            [Test]
            public void ShouldSetQ()
            {
                var q = new BigInteger(rawQ);
                var keyParameters = (DsaPublicKeyParameters)PublicKeyFactory.CreateKey(key.Content);
                
                Assert.AreEqual(keyParameters.Parameters.Q, q);
            }
            
            [Test]
            public void ShouldSetG()
            {
                var g = new BigInteger(rawG);
                var keyParameters = (DsaPublicKeyParameters)PublicKeyFactory.CreateKey(key.Content);
                
                Assert.AreEqual(keyParameters.Parameters.G, g);
            }
            
            [Test]
            public void ShouldSetY()
            {
                var y = new BigInteger(rawY);
                var keyParameters = (DsaPublicKeyParameters)PublicKeyFactory.CreateKey(key.Content);
                
                Assert.AreEqual(keyParameters.Y, y);
            }
        }

        [TestFixture]
        public class GetEcPublicKeyContent : SshKeyProviderTest
        {
            private IEcKey key;
            private byte[] result;

            private byte[] rawIdentifier;
            private byte[] rawHeader;
            private byte[] rawQ;
            
            private readonly Dictionary<string, string> sshCurveHeaders = new Dictionary<string, string>
            {
                {"curve25519", "ed25519"},
                {"P-256", "nistp256"},
                {"P-384", "nistp384"},
                {"P-521", "nistp521"}
            };
            
            private readonly Dictionary<string, string> sshCurveIdentifiers = new Dictionary<string, string>
            {
                {"curve25519", "ssh-ed25519"},
                {"P-256", "ecdsa-sha2-nistp256"},
                {"P-384", "ecdsa-sha2-nistp384"},
                {"P-521", "ecdsa-sha2-nistp521"}
            };
            
            private void SetupForCurve(string curveName)
            {
                var ecKeyProvider = new EcKeyProvider(keyPairGenerator, new FieldToCurveNameMapper());
                IAsymmetricKeyPair keyPair = ecKeyProvider.CreateKeyPair(curveName);
                key = (IEcKey)keyPair.PublicKey;

                string keyContent = provider.GetEcPublicKeyContent(key);
                result = base64.FromBase64String(keyContent);

                using (var stream = new MemoryStream(result))
                {
                    rawIdentifier = ReadNextContent(stream);
                    if (curveName != "curve25519")
                    {
                        rawHeader = ReadNextContent(stream);
                    }
                    rawQ = ReadNextContent(stream);
                }
            }

            [TestCase("curve25519")]
            [TestCase("P-256")]
            [TestCase("P-384")]
            [TestCase("P-521")]
            public void ShouldSetIdentifier(string curve)
            {
                SetupForCurve(curve);
                string identifier = Encoding.UTF8.GetString(rawIdentifier);
                Assert.AreEqual(sshCurveIdentifiers[curve], identifier);
            }
            
            [TestCase("P-256")]
            [TestCase("P-384")]
            [TestCase("P-521")]
            public void ShouldSetHeader(string curve)
            {
                SetupForCurve(curve);
                string headerContent = Encoding.UTF8.GetString(rawHeader);
                Assert.AreEqual(sshCurveHeaders[curve], headerContent);
            }

            [TestCase("P-256")]
            [TestCase("P-384")]
            [TestCase("P-521")]
            public void ShouldSetQ(string curve)
            {
                SetupForCurve(curve);
                X9ECParameters curveParameters = ECNamedCurveTable.GetByName(curve) ?? CustomNamedCurves.GetByName(curve);
                var ecDomainParameters = new ECDomainParameters(curveParameters.Curve,
                                                                curveParameters.G,
                                                                curveParameters.N,
                                                                curveParameters.H,
                                                                curveParameters.GetSeed());

                ECPoint qPoint = ecDomainParameters.Curve.DecodePoint(rawQ);

                var expected = (ECPublicKeyParameters)PublicKeyFactory.CreateKey(key.Content);
                Assert.AreEqual(expected.Q, qPoint);
            }

            [Test]
            public void ShouldSetQForCurve25519InEdwardsForm()
            {
                SetupForCurve("curve25519");
                var keyParameters = (ECPublicKeyParameters)PublicKeyFactory.CreateKey(key.Content);
                byte[] expected = ecKeyProvider.GetEd25519PublicKeyFromCurve25519(keyParameters.Q.GetEncoded());
                
                CollectionAssert.AreEqual(expected, rawQ);
            }
        }

        [TestFixture]
        public class GetKeyFromSsh : SshKeyProviderTest
        {
            private RsaKeyProvider rsaKeyProvider;
            private DsaKeyProvider dsaKeyProvider;
            private EcKeyProvider ecKeyProvider;

            private IAsymmetricKey key;
            private IAsymmetricKey result;
            
            [SetUp]
            public void SetupGetKeyFromSsh()
            {
                var asymmetricKeyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
                rsaKeyProvider = new RsaKeyProvider(asymmetricKeyPairGenerator);
                dsaKeyProvider = new DsaKeyProvider(asymmetricKeyPairGenerator);
                ecKeyProvider = new EcKeyProvider(asymmetricKeyPairGenerator, new FieldToCurveNameMapper());
                
                provider = new SshKeyProvider(new EncodingWrapper(), new Base64Wrapper(), rsaKeyProvider, dsaKeyProvider, ecKeyProvider);
            }
            
            
            [TestFixture]
            public class Rsa : GetKeyFromSsh
            {
                [SetUp]
                public void Setup()
                {
                    IAsymmetricKeyPair keyPair = rsaKeyProvider.CreateKeyPair(2048);
                    key = keyPair.PublicKey;

                    string keyContent = provider.GetRsaPublicKeyContent(key);
                    result = provider.GetKeyFromSsh(keyContent);
                }

                [Test]
                public void ShouldReturnValidRsaKey()
                {
                    Assert.AreEqual(key.Content, result.Content);
                }
            }

            [TestFixture]
            public class Dsa : GetKeyFromSsh
            {
                [SetUp]
                public void Setup()
                {
                    IAsymmetricKeyPair keyPair = dsaKeyProvider.CreateKeyPair(2048);
                    key = keyPair.PublicKey;

                    string keyContent = provider.GetDsaPublicKeyContent(key);
                    result = provider.GetKeyFromSsh(keyContent);
                }

                [Test]
                public void ShouldReturnValidDsaKey()
                {
                    Assert.AreEqual(key.Content, result.Content);
                }
            }

            [TestFixture]
            public class Ec : GetKeyFromSsh
            {
                private string sshKeyContent;

                private void SetupForCurve(string curve)
                {
                    IAsymmetricKeyPair keyPair = ecKeyProvider.CreateKeyPair(curve);
                    key = keyPair.PublicKey;

                    sshKeyContent = provider.GetEcPublicKeyContent(key);
                    result = provider.GetKeyFromSsh(sshKeyContent);
                }

                [TestCase("P-256")]
                [TestCase("P-384")]
                [TestCase("P-521")]
                public void ShouldReturnValidEcKey(string curve)
                {
                    SetupForCurve(curve);
                    Assert.AreEqual(key.Content, result.Content);
                }

                [Test]
                public void ShouldThrowExceptionWhenHeaderIsNotIdentified()
                {
                    SetupForCurve("P-256");
                    byte[] content = base64.FromBase64String(sshKeyContent);
                    content[4] = (byte)(content[4] >> 1);

                    string alteredContent = base64.ToBase64String(content);

                    Assert.Throws<ArgumentException>(() => provider.GetKeyFromSsh(alteredContent));
                }

                [Test]
                public void ShouldThrowOnNonSupportedCurve()
                {
                    Assert.Throws<ArgumentException>(() => SetupForCurve("curve25519"));
                }
            }
        }
    }
}