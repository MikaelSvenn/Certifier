using System;
using System.Linq;
using System.Threading;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Crypto.Formatters;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using Moq;
using NUnit.Framework;

namespace Crypto.Test.Providers
{
    [TestFixture]
    public class SshFormattingProviderTest
    {
        private SshFormattingProvider provider;
        private Mock<Ssh2ContentFormatter> contentFormatter;
        private Mock<ISshKeyProvider> keyProvider;
        
        [SetUp]
        public void SetupSshFormattingProviderTest()
        {
            keyProvider = new Mock<ISshKeyProvider>();
            contentFormatter = new Mock<Ssh2ContentFormatter>();
            contentFormatter.Setup(c => c.FormatToSsh2Header(It.IsAny<string>()))
                             .Returns<string>(s => $"formatted {s}");
            contentFormatter.Setup(c => c.FormatToSsh2KeyContent(It.IsAny<string>()))
                             .Returns<string>(s => $"formattedkey {s}");
            
            provider = new SshFormattingProvider(keyProvider.Object, new EncodingWrapper(), contentFormatter.Object, new Base64Wrapper());
        }

        [TestFixture]
        public class GetAsOpenSsh : SshFormattingProviderTest
        {
            private string rawResult;
            private string[] result;

            [Test]
            public void ShouldThrowExceptionWhenPrivateKeyIsGiven()
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
                Assert.Throws<InvalidOperationException>(() => provider.GetAsOpenSsh(key, "foobarcomment"));
            }

            [Test]
            public void ShouldThrowExceptionWhenCipherTypeIsNotSupported()
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.CipherType == CipherType.ElGamal);
                Assert.Throws<InvalidOperationException>(() => provider.GetAsOpenSsh(key, "foobarcomment"));
            }
            
            [TestFixture]
            public class EllipticCurveKey : GetAsOpenSsh
            {
                private IEcKey key;
                
                [SetUp]
                public void Setup()
                {
                    key = Mock.Of<IEcKey>(k => k.Curve == "foo" && k.CipherType == CipherType.Ec);
                    
                    keyProvider.Setup(kp => kp.IsSupportedCurve("foo"))
                               .Returns(true);
                    keyProvider.Setup(kp => kp.GetCurveSshHeader("foo"))
                               .Returns("fooHeader");
                    keyProvider.Setup(kp => kp.GetEcPublicKeyContent(key))
                               .Returns("EcKeyContent");
                    
                    rawResult  = provider.GetAsOpenSsh(key, "eccomment");
                    result = rawResult.Split(' ');
                }

                [Test]
                public void ShouldThrowExceptionWhenCurveIsNotSupported()
                {
                    key = Mock.Of<IEcKey>(k => k.Curve == "bar" && k.CipherType == CipherType.Ec);
                    Assert.Throws<ArgumentException>(() => provider.GetAsOpenSsh(key, "comment"));
                }

                [Test]
                public void ShouldSetHeader()
                {
                    Assert.AreEqual("fooHeader", result[0]);
                }

                [Test]
                public void ShouldSetContent()
                {
                    Assert.AreEqual("EcKeyContent", result[1]);
                }

                [Test]
                public void ShouldSetComment()
                {
                    Assert.AreEqual("eccomment", result[2]);
                }
                
                [Test]
                public void ShouldNotContainLineBreaks()
                {
                    Assert.IsFalse(rawResult.Contains('\n'));
                }
                
                [Test]
                public void ShouldFormatKeyInThreeSections()
                {
                    Assert.AreEqual(3, result.Length);
                }
            }

            [TestFixture]
            public class RsaKey : GetAsOpenSsh
            {
                private IAsymmetricKey key;
                
                [SetUp]
                public void Setup()
                {
                    key = Mock.Of<IAsymmetricKey>(k => k.CipherType == CipherType.Rsa);
                    keyProvider.Setup(kp => kp.GetRsaPublicKeyContent(key))
                               .Returns("RsaKeyContent");
                    
                    rawResult  = provider.GetAsOpenSsh(key, "rsacomment");
                    result = rawResult.Split(' ');
                }
                
                [Test]
                public void ShouldSetHeader()
                {
                    Assert.AreEqual("ssh-rsa", result[0]);
                }

                [Test]
                public void ShouldSetContent()
                {
                    Assert.AreEqual("RsaKeyContent", result[1]);
                }

                [Test]
                public void ShouldSetComment()
                {
                    Assert.AreEqual("rsacomment", result[2]);
                }
                
                [Test]
                public void ShouldNotContainLineBreaks()
                {
                    Assert.IsFalse(rawResult.Contains('\n'));
                }
                
                [Test]
                public void ShouldFormatKeyInThreeSections()
                {
                    Assert.AreEqual(3, result.Length);
                }
            }

            [TestFixture]
            public class DsaKey : GetAsOpenSsh
            {
                private IAsymmetricKey key;
                
                [SetUp]
                public void Setup()
                {
                    key = Mock.Of<IAsymmetricKey>(k => k.CipherType == CipherType.Dsa);
                    keyProvider.Setup(kp => kp.GetDsaPublicKeyContent(key))
                               .Returns("DsaKeyContent");
                    
                    rawResult  = provider.GetAsOpenSsh(key, "dsacomment");
                    result = rawResult.Split(' ');
                }
                
                [Test]
                public void ShouldSetHeader()
                {
                    Assert.AreEqual("ssh-dss", result[0]);
                }

                [Test]
                public void ShouldSetContent()
                {
                    Assert.AreEqual("DsaKeyContent", result[1]);
                }

                [Test]
                public void ShouldSetComment()
                {
                    Assert.AreEqual("dsacomment", result[2]);
                }
                
                [Test]
                public void ShouldNotContainLineBreaks()
                {
                    Assert.IsFalse(rawResult.Contains('\n'));
                }
                
                [Test]
                public void ShouldFormatKeyInThreeSections()
                {
                    Assert.AreEqual(3, result.Length);
                }
            }
        }
        
        [TestFixture]
        public class GetAsSsh2 : SshFormattingProviderTest
        {
            private string rawContent;
            private string[] result;
            
            [Test]
            public void ShouldThrowExceptionWhenPrivateKeyIsGiven()
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
                Assert.Throws<InvalidOperationException>(() => provider.GetAsSsh2(key, ""));
            }
           
            [Test]
            public void ShouldThrowExceptionWhenCipherTypeIsNotSupported()
            {
                var key = Mock.Of<IAsymmetricKey>(k => k.CipherType == CipherType.ElGamal);
                Assert.Throws<InvalidOperationException>(() => provider.GetAsSsh2(key, "foo"));
            }
            
            [TestFixture]
            public class EcKey : GetAsSsh2
            {
                private IEcKey key;
                
                [SetUp]
                public void Setup()
                {
                    key = Mock.Of<IEcKey>(k => k.CipherType == CipherType.Ec && k.Curve == "foo");
                    
                    keyProvider.Setup(kp => kp.GetEcPublicKeyContent(key))
                               .Returns("ECKeyContent");
                    keyProvider.Setup(kp => kp.IsSupportedCurve("foo"))
                               .Returns(true);
                    
                    rawContent  = provider.GetAsSsh2(key, "eccomment");
                    result = rawContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                }

                [Test]
                public void ShouldThrowWhenCurveIsNotSupported()
                {
                    key = Mock.Of<IEcKey>(k => k.CipherType == CipherType.Ec && k.Curve == "bar");
                    Assert.Throws<ArgumentException>(() => provider.GetAsSsh2(key, "foobar"));
                }
                
                [Test]
                public void ShouldFormatKeyContent()
                {
                    Assert.AreEqual("formattedkey ECKeyContent", result[2]);
                }
                
                [Test]
                public void ShouldFormatCommentHeader()
                {
                    Assert.AreEqual("Comment: formatted eccomment", result[1]);
                }
                
                [Test]
                public void ShouldFormatKeyInFourSections()
                {
                    Assert.AreEqual(4, result.Length);
                }

                [Test]
                public void ShouldHaveBeginDescriptor()
                {
                    Assert.AreEqual("---- BEGIN SSH2 PUBLIC KEY ----", result[0]);
                }

                [Test]
                public void ShouldHaveEndDescriptor()
                {
                    Assert.AreEqual("---- END SSH2 PUBLIC KEY ----", result[3]);
                }

                [Test]
                public void ShouldThrowWhenCommentExceeds1024Bytes()
                {
                    string comment = string.Concat(Enumerable.Repeat("a", 1500));
                    Assert.Throws<ArgumentException>(() => provider.GetAsSsh2(key, comment));
                }
            }

            [TestFixture]
            public class RsaKey : GetAsSsh2
            {
                private IAsymmetricKey key;
                
                [SetUp]
                public void Setup()
                {
                    key = Mock.Of<IAsymmetricKey>(k => k.CipherType == CipherType.Rsa);
                    keyProvider.Setup(kp => kp.GetRsaPublicKeyContent(key))
                               .Returns("RsaKeyContent");
                    
                    rawContent  = provider.GetAsSsh2(key, "rsacomment");
                    result = rawContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                }
               
                [Test]
                public void ShouldFormatKeyContent()
                {
                    Assert.AreEqual("formattedkey RsaKeyContent", result[2]);
                }
                
                [Test]
                public void ShouldFormatCommentHeader()
                {
                    Assert.AreEqual("Comment: formatted rsacomment", result[1]);
                }
                
                [Test]
                public void ShouldFormatKeyInFourSections()
                {
                    Assert.AreEqual(4, result.Length);
                }

                [Test]
                public void ShouldHaveBeginDescriptor()
                {
                    Assert.AreEqual("---- BEGIN SSH2 PUBLIC KEY ----", result[0]);
                }

                [Test]
                public void ShouldHaveEndDescriptor()
                {
                    Assert.AreEqual("---- END SSH2 PUBLIC KEY ----", result[3]);
                }

                [Test]
                public void ShouldThrowWhenCommentExceeds1024Bytes()
                {
                    string comment = string.Concat(Enumerable.Repeat("a", 1500));
                    Assert.Throws<ArgumentException>(() => provider.GetAsSsh2(key, comment));
                }
            }

            [TestFixture]
            public class DsaKey : GetAsSsh2
            {
                private IAsymmetricKey key;
                
                [SetUp]
                public void Setup()
                {
                    key = Mock.Of<IAsymmetricKey>(k => k.CipherType == CipherType.Dsa);
                    keyProvider.Setup(kp => kp.GetDsaPublicKeyContent(key))
                               .Returns("DsaKeyContent");
                    
                    rawContent  = provider.GetAsSsh2(key, "dsacomment");
                    result = rawContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                }
                
                [Test]
                public void ShouldFormatKeyContent()
                {
                    Assert.AreEqual("formattedkey DsaKeyContent", result[2]);
                }
                
                [Test]
                public void ShouldFormatCommentHeader()
                {
                    Assert.AreEqual("Comment: formatted dsacomment", result[1]);
                }
                
                [Test]
                public void ShouldFormatKeyInFourSections()
                {
                    Assert.AreEqual(4, result.Length);
                }

                [Test]
                public void ShouldHaveBeginDescriptor()
                {
                    Assert.AreEqual("---- BEGIN SSH2 PUBLIC KEY ----", result[0]);
                }

                [Test]
                public void ShouldHaveEndDescriptor()
                {
                    Assert.AreEqual("---- END SSH2 PUBLIC KEY ----", result[3]);
                }

                [Test]
                public void ShouldThrowWhenCommentExceeds1024Bytes()
                {
                    string comment = string.Concat(Enumerable.Repeat("a", 1500));
                    Assert.Throws<ArgumentException>(() => provider.GetAsSsh2(key, comment));
                }
            }
        }

        [TestFixture]
        public class GetAsDer : SshFormattingProviderTest
        {          
            private string ssh2Key;
            private string openSshKey;
            private string keyContent;

            [SetUp]
            public void SetupGetAsDer()
            {
                var secureRandom = new SecureRandomGenerator();
                var asymmetricKeyPairGenerator = new AsymmetricKeyPairGenerator(secureRandom);
                var rsaKeyProvider = new RsaKeyProvider(asymmetricKeyPairGenerator);
                var dsaKeyProvider = new DsaKeyProvider(asymmetricKeyPairGenerator);
                var ecKeyProvier = new EcKeyProvider(asymmetricKeyPairGenerator, new FieldToCurveNameMapper());
                var keyPair = rsaKeyProvider.CreateKeyPair(1024);
                var key = keyPair.PublicKey;
                
                var encoding = new EncodingWrapper();
                var base64 = new Base64Wrapper();
                var sshKeyProvider = new SshKeyProvider(encoding, base64, rsaKeyProvider, dsaKeyProvider, ecKeyProvier);
                
                var formattingProvider = new SshFormattingProvider(sshKeyProvider, encoding, new Ssh2ContentFormatter(), base64);

                ssh2Key = formattingProvider.GetAsSsh2(key, "foo");
                keyContent = sshKeyProvider.GetRsaPublicKeyContent(key);
                openSshKey = formattingProvider.GetAsOpenSsh(key, "foo");
            }
            
            [TestFixture]
            public class WhenKeyIsSsh2Formatted : GetAsDer
            {
                private IAsymmetricKey key;
                
                [SetUp]
                public void Setup()
                {
                    key = Mock.Of<IAsymmetricKey>();
                    keyProvider.Setup(kp => kp.GetKeyFromSsh(keyContent))
                               .Returns(key);
                }

                [Test]
                public void ShouldGetKeyByContent()
                {
                    var result = provider.GetAsDer(ssh2Key);
                    Assert.AreEqual(key, result);
                }
            }

            [TestFixture]
            public class WhenKeyIsOpenSshFormatted : GetAsDer
            {
                private IAsymmetricKey key;
                
                [SetUp]
                public void Setup()
                {
                    key = Mock.Of<IAsymmetricKey>();
                    keyProvider.Setup(kp => kp.GetKeyFromSsh(keyContent))
                               .Returns(key);
                }

                [Test]
                public void ShouldGetKeyByContent()
                {
                    var result = provider.GetAsDer(openSshKey);
                    Assert.AreEqual(key, result);
                }
            }
        }

        [TestFixture]
        public class IsSshKey : SshFormattingProviderTest
        {
            [TestCase("---- BEGIN SSH2 PUBLIC")]
            [TestCase("ssh-rsa ")]
            [TestCase("ssh-dss ")]
            [TestCase("ssh-ed25519 ")]
            [TestCase("ecdsa-sha2-nistp256 ")]
            [TestCase("ecdsa-sha2-nistp384 ")]
            [TestCase("ecdsa-sha2-nistp521 ")]
            public void ShouldReturnTrueForKnownHeader(string header)
            {
                Assert.IsTrue(provider.IsSshKey(header));
            }

            [TestCase("")]
            [TestCase(" ")]
            [TestCase("foobarbaz")]
            [TestCase("---- BEGIN SSH2 PRIVATE")]
            [TestCase("ssh-ed25519-cert-v01@openssh.com")]
            [TestCase("ssh-rsa-cert-v01@openssh.com")]
            [TestCase("ecdsa-sha2-nistp384-cert-v01@openssh.com")]
            public void ShouldReturnFalseForUnknownHeader(string header)
            {
                Assert.IsFalse(provider.IsSshKey(header));
            }
        }
    }
}