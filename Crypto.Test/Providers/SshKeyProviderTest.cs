using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Core.Interfaces;
using Core.Model;
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
        private EncodingWrapper encoding;
        
        private readonly Dictionary<string, string> sshCurveIdentifiers = new Dictionary<string, string>
        {
            {"curve25519", "ssh-ed25519"},
            {"P-256", "ecdsa-sha2-nistp256"},
            {"P-384", "ecdsa-sha2-nistp384"},
            {"P-521", "ecdsa-sha2-nistp521"}
        };
        
        [OneTimeSetUp]
        public void SetupSshKeyProviderTest()
        {
            encoding = new EncodingWrapper();
            keyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            ecKeyProvider = new EcKeyProvider(keyPairGenerator, new FieldToCurveNameMapper());
            provider = new SshKeyProvider(encoding, new Base64Wrapper(), Mock.Of<IRsaKeyProvider>(), Mock.Of<IDsaKeyProvider>(), ecKeyProvider, new SecureRandomGenerator());
            
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
                string headerContent = encoding.GetString(rawHeader);
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
                string headerContent = encoding.GetString(rawHeader);
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
            private IEcKey publicKey;
            private byte[] result;

            private byte[] rawIdentifier;
            private byte[] rawHeader;
            private byte[] rawQ;
            
            private readonly Dictionary<string, string> sshCurveHeaders = new Dictionary<string, string>
            {
                {"P-256", "nistp256"},
                {"P-384", "nistp384"},
                {"P-521", "nistp521"}
            };
            
            private void SetupForCurve(string curveName)
            {
                IAsymmetricKeyPair keyPair = ecKeyProvider.CreateKeyPair(curveName);
                publicKey = (IEcKey)keyPair.PublicKey;
                
                string keyContent = provider.GetEcPublicKeyContent(publicKey);
                result = base64.FromBase64String(keyContent);

                using (var stream = new MemoryStream(result))
                {
                    rawIdentifier = ReadNextContent(stream);
                    rawHeader = ReadNextContent(stream);
                    rawQ = ReadNextContent(stream);
                }
            }

            [TestCase("P-256")]
            [TestCase("P-384")]
            [TestCase("P-521")]
            public void ShouldSetIdentifier(string curve)
            {
                SetupForCurve(curve);
                string identifier = encoding.GetString(rawIdentifier);
                Assert.AreEqual(sshCurveIdentifiers[curve], identifier);
            }
            
            [TestCase("P-256")]
            [TestCase("P-384")]
            [TestCase("P-521")]
            public void ShouldSetHeader(string curve)
            {
                SetupForCurve(curve);
                string headerContent = encoding.GetString(rawHeader);
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

                var expected = (ECPublicKeyParameters)PublicKeyFactory.CreateKey(publicKey.Content);
                Assert.AreEqual(expected.Q, qPoint);
            }

            [Test]
            public void ShouldThrowWhenKeyCurveIs25519()
            {
                Assert.Throws<InvalidOperationException>(() => SetupForCurve("curve25519"));
            }
        }

        [TestFixture]
        public class GetEd25519PublicKeyContent : SshKeyProviderTest
        {
            private IAsymmetricKeyPair keyPair;
            private byte[] rawIdentifier;
            private byte[] rawQ;

            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = ecKeyProvider.CreateKeyPair("curve25519");
                
                string keyContent = provider.GetEd25519PublicKeyContent(keyPair.PrivateKey);
                byte[] result = base64.FromBase64String(keyContent);

                using (var stream = new MemoryStream(result))
                {
                    rawIdentifier = ReadNextContent(stream);
                    rawQ = ReadNextContent(stream);
                }
            }
            
            [Test]
            public void ShouldSetIdentifier()
            {
                string identifier = encoding.GetString(rawIdentifier);
                Assert.AreEqual("ssh-ed25519", identifier);
            }

            [Test]
            public void ShouldSetQInEdwardsForm()
            {
                byte[] expected = ecKeyProvider.GetEd25519PublicKeyFromCurve25519(keyPair.PrivateKey);
                Assert.AreEqual(expected, rawQ);
            }
        }
        
        [TestFixture]
        public class GetKeyFromSsh : SshKeyProviderTest
        {
            private RsaKeyProvider rsaKeyProvider;
            private DsaKeyProvider dsaKeyProvider;
            private IAsymmetricKey key;
            private IAsymmetricKey result;
            
            [SetUp]
            public void SetupGetKeyFromSsh()
            {
                var asymmetricKeyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
                rsaKeyProvider = new RsaKeyProvider(asymmetricKeyPairGenerator);
                dsaKeyProvider = new DsaKeyProvider(asymmetricKeyPairGenerator);
                
                provider = new SshKeyProvider(new EncodingWrapper(), new Base64Wrapper(), rsaKeyProvider, dsaKeyProvider, ecKeyProvider, null);
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
                    IAsymmetricKeyPair keyPair = ecKeyProvider.CreateKeyPair("curve25519");

                    sshKeyContent = provider.GetEd25519PublicKeyContent(keyPair.PrivateKey);
                    Assert.Throws<ArgumentException>(() => provider.GetKeyFromSsh(sshKeyContent));
                }
            }
        }

        [TestFixture]
        public class GetOpenSshEd25519PrivateKey : SshKeyProviderTest
        {
            private string encodedResult;
            private byte[] result;
            private IAsymmetricKeyPair keyPair;
            private byte[] publicKeyContent;
            private readonly int headerLength = 94;
            
            [OneTimeSetUp]
            public void Setup()
            {
                keyPair = ecKeyProvider.CreateKeyPair("curve25519");
                publicKeyContent = ecKeyProvider.GetEd25519PublicKeyFromCurve25519(keyPair.PrivateKey);

                encodedResult = provider.GetOpenSshEd25519PrivateKey(keyPair, "comment");
                result = base64.FromBase64String(encodedResult);
            }
            
            [Test]
            public void ShouldThrowWhenPrivateKeyIsEncrypted()
            {
                var privateKey = Mock.Of<IEcKey>(k => k.IsEncrypted);
                keyPair = new AsymmetricKeyPair(privateKey, null);
                var exception = Assert.Throws<InvalidOperationException>(() => provider.GetOpenSshEd25519PrivateKey(keyPair, "comment"));
                Assert.AreEqual("Only non-encrypted ed25519 keys are supported.", exception.Message);
            }

            [Test]
            public void ShouldThrowWhenKeyIsNotCurve25519()
            {
                var privateKey = Mock.Of<IEcKey>(k => !k.IsCurve25519);
                keyPair = new AsymmetricKeyPair(privateKey, null);
                var exception = Assert.Throws<InvalidOperationException>(() => provider.GetOpenSshEd25519PrivateKey(keyPair, "comment"));
                Assert.AreEqual("Only non-encrypted ed25519 keys are supported.", exception.Message);
            }

            [Test]
            public void ShouldSetOpenSshVersionHeaderWithExplicitNullTerminator()
            {
                byte[] headerContent = result.Take(15).ToArray();
                string header = encoding.GetString(headerContent);
                Assert.AreEqual("openssh-key-v1\0", header);
            }

            [Test]
            public void ShouldSetCipherNameLengthInBigEndian()
            {
                byte[] cipherNameLength = result.Skip(15)
                                                .Take(4)
                                                .ToArray();

                EnsureBigEndian(ref cipherNameLength);               
                Assert.AreEqual(4, BitConverter.ToInt32(cipherNameLength, 0));
            }

            private void EnsureBigEndian(ref byte[] content)
            {
                if (BitConverter.IsLittleEndian)
                {
                    content = content.Reverse()
                                     .ToArray();
                }
            }
            
            [Test]
            public void ShouldSetCipherNameToNone()
            {
                byte[] cipherNameContent = result.Skip(19)
                                                 .Take(4)
                                                 .ToArray();
                
                string cipherName = encoding.GetString(cipherNameContent);
                Assert.AreEqual("none", cipherName);
            }

            [Test]
            public void ShouldSetKdfLengthInBigEndian()
            {
                byte[] kdfLength = result.Skip(23)
                                         .Take(4)
                                         .ToArray();

                EnsureBigEndian(ref kdfLength);
                Assert.AreEqual(4, BitConverter.ToInt32(kdfLength, 0));
            }
            
            [Test]
            public void ShouldSetKdfToNone()
            {
                byte[] kdfContent = result.Skip(27)
                                          .Take(4)
                                          .ToArray();
                
                string kdf = encoding.GetString(kdfContent);
                Assert.AreEqual("none", kdf);
            }

            [Test]
            public void ShouldSetKdfOptionsLengthInBigEndian()
            {
                byte[] optionsLength = result.Skip(31)
                                             .Take(4)
                                             .ToArray();

                EnsureBigEndian(ref optionsLength);
                Assert.AreEqual(0, BitConverter.ToInt32(optionsLength, 0));
            }
            
            [Test]
            public void ShouldSetKdfOptionsToEmptyString()
            {
                byte[] kdfOptions = result.Skip(35)
                                          .Take(1)
                                          .ToArray();
                
                string options = encoding.GetString(kdfOptions);
                Assert.AreEqual("\0", options);
            }

            [Test]
            public void ShouldSetNumberOfKeysTo1InBigEndian()
            {
                byte[] numberOfKeys = result.Skip(35)
                                            .Take(4)
                                            .ToArray();

                EnsureBigEndian(ref numberOfKeys);
                Assert.AreEqual(1, BitConverter.ToInt32(numberOfKeys, 0));
            }

            [Test]
            public void ShouldSetCombinedLengthOfPublicKeyHeaderAndPublicKeyInBigEndian()
            {
                byte[] publicKeyWithHeaderLength = result.Skip(39)
                                                         .Take(4)
                                                         .ToArray();

                EnsureBigEndian(ref publicKeyWithHeaderLength);
                int expected = 4 + 11 + 4 + publicKeyContent.Length;
                Assert.AreEqual(expected, BitConverter.ToInt32(publicKeyWithHeaderLength, 0));
            }

            [Test]
            public void ShouldSetPublicKey()
            {
                byte[] identifier = encoding.GetBytes("ssh-ed25519");
                byte[] identifierLength = BitConverter.GetBytes(identifier.Length);
                EnsureBigEndian(ref identifierLength);

                byte[] publicKeyLength = BitConverter.GetBytes(publicKeyContent.Length);
                EnsureBigEndian(ref publicKeyLength);

                var expected1 = new List<byte>();
                expected1.AddRange(identifierLength);
                expected1.AddRange(identifier);
                expected1.AddRange(publicKeyLength);
                expected1.AddRange(publicKeyContent);
                List<byte> expected = expected1;
                
                byte[] publicKey = result.Skip(43)
                                         .Take(19 + publicKeyContent.Length)
                                         .ToArray();
                                
                CollectionAssert.AreEqual(expected.ToArray(), publicKey);
            }

            [Test]
            public void ShouldSetContentContainerLengthInBigEndian()
            {
                byte[] contentLength = result.Skip(headerLength)
                                             .Take(4)
                                             .ToArray();

                if (BitConverter.IsLittleEndian)
                {
                    contentLength = contentLength.Reverse().ToArray();
                }
                
                Assert.AreEqual(144, BitConverter.ToInt32(contentLength, 0));
            }
            
            [Test]
            public void ShouldSetSameChecksumTwice()
            {
                byte[] publicKey = result.Skip(headerLength + 4)
                                         .Take(8)
                                         .ToArray();

                int checksum = BitConverter.ToInt32(publicKey, 0);
                int duplicateChecksum = BitConverter.ToInt32(publicKey, 4);
                
                Assert.AreEqual(checksum, duplicateChecksum);
            }

            [Test]
            public void ShouldSetPublicKeyHeaderLengthInBigEndian()
            {
                byte[] publicKeyHeaderLength = result.Skip(headerLength + 12)
                                                         .Take(4)
                                                         .ToArray();
                
                if (BitConverter.IsLittleEndian)
                {
                    publicKeyHeaderLength = publicKeyHeaderLength.Reverse().ToArray();
                }
                
                Assert.AreEqual("ssh-ed25519".Length, BitConverter.ToInt32(publicKeyHeaderLength, 0));
            }

            [Test]
            public void ShouldSetPublicKeyHeader()
            {
                byte[] publicKeyHeader = result.Skip(headerLength + 16)
                                         .Take(11)
                                         .ToArray();

                byte[] expected = encoding.GetBytes("ssh-ed25519");
                CollectionAssert.AreEqual(expected, publicKeyHeader);
            }

            [Test]
            public void ShouldSetPublicKeyLengthInBigEndian()
            {
                byte[] publicKeyLength = result.Skip(headerLength + 27)
                                                .Take(4)
                                                .ToArray();
                
                if (BitConverter.IsLittleEndian)
                {
                    publicKeyLength = publicKeyLength.Reverse().ToArray();
                }
                
                Assert.AreEqual(32, BitConverter.ToInt32(publicKeyLength, 0));
            }

            [Test]
            public void ShouldSetPublicKeyAgain()
            {
                byte[] expected = ecKeyProvider.GetEd25519PublicKeyFromCurve25519(keyPair.PrivateKey);
                byte[] publicKey = result.Skip(headerLength + 31)
                                          .Take(32)
                                          .ToArray();
                
                Assert.AreEqual(expected, publicKey);
            }

            [Test]
            public void ShouldSetPrivateKeyLengthInBigEndian()
            {
                byte[] privateKeyLength = result.Skip(headerLength + 63)
                                          .Take(4)
                                          .ToArray();
                
                if (BitConverter.IsLittleEndian)
                {
                    privateKeyLength = privateKeyLength.Reverse().ToArray();
                }
                
                Assert.AreEqual(64, BitConverter.ToInt32(privateKeyLength, 0));
            }

            [Test]
            public void ShouldSetPrivateKey()
            {
                var privateKeyParameters = (ECPrivateKeyParameters)PrivateKeyFactory.CreateKey(keyPair.PrivateKey.Content);
                
                byte[] d = privateKeyParameters.D.ToByteArray();
                byte[] q = ecKeyProvider.GetEd25519PublicKeyFromCurve25519(keyPair.PrivateKey);

                byte[] expected = d.Concat(q)
                                   .ToArray();
                
                byte[] privateKey = result.Skip(headerLength + 67)
                                          .Take(64)
                                          .ToArray();
                
                Assert.AreEqual(expected, privateKey);
            }
            
            [Test]
            public void ShouldSetCommentLengthInBigEndian()
            {
                int expectedLength = encoding.GetBytes("comment").Length;
                
                byte[] commentLength = result.Skip(headerLength + 131)
                                         .Take(4)
                                         .ToArray();
                
                if (BitConverter.IsLittleEndian)
                {
                    commentLength = commentLength.Reverse().ToArray();
                }
                
                Assert.AreEqual(expectedLength, BitConverter.ToInt32(commentLength, 0));
            }

            [Test]
            public void ShouldSetComment()
            {
                byte[] commentContent = result.Skip(headerLength + 135)
                                              .Take(7)
                                              .ToArray();

                string comment = encoding.GetString(commentContent);
                Assert.AreEqual("comment", comment);
            }

            [Test]
            public void ShouldAddPaddingByCipherBlockSize()
            {
                var expectedPadding = new byte[] {1, 2, 3, 4, 5, 6};
                
                byte[] padding = result.Skip(headerLength + 142)
                                       .ToArray();

                CollectionAssert.AreEqual(expectedPadding, padding);
            }
        }
    }
}