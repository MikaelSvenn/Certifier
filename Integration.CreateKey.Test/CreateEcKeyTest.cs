﻿using System;
using System.Collections.Generic;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Crypto.Providers;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using SimpleInjector;
using Ui.Console;
using Ui.Console.Provider;

namespace Integration.CreateKey.Test
{
    [TestFixture]
    public class CreateEcKeyTest
    {
        private Dictionary<string, byte[]> fileOutput;
        private Mock<FileWrapper> file;
        private EncodingWrapper encoding;

        [SetUp]
        public void SetupCreateEcKeyTest()
        {
            encoding = new EncodingWrapper();
            fileOutput = new Dictionary<string, byte[]>();

            file = new Mock<FileWrapper>();
            file.Setup(f => f.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()))
                 .Callback<string, byte[]>((path, content) =>
                 {
                     fileOutput.Add(path, content);
                 });

            Container container = ContainerProvider.GetContainer();
            container.Register<FileWrapper>(() => file.Object);
        }
        
        [TearDown]
        public void TeardownCreateEcKeyTest()
        {
            ContainerProvider.ClearContainer();
        }
        
        [TestFixture]
        public class ErrorCases : CreateEcKeyTest
        {
            [Test]
            public void ShouldIndicateMissingPrivateKey()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-k", "ec", "--publickey", "public"}); });
                Assert.AreEqual("Private key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPublicKey()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-k", "ec", "--privatekey", "private"}); });
                Assert.AreEqual("Public key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPassword()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-k", "ec", "-e", "pkcs", "--privatekey", "private.pem", "--publickey", "public.pem"}); });
                Assert.AreEqual("Password is required for encryption.", exception.Message);
            }

            [Test]
            public void ShouldIndicateNonSupportedCurve()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", "foo", "-e", "pkcs", "--privatekey", "private.pem", "--publickey", "public.pem"}); });
                Assert.AreEqual("Curve foo is not supported.", exception.Message);
            }

            [TestCase("K-233")]
            [TestCase("sect193r2")]
            [TestCase("prime239v3")]
            [TestCase("c2tnb431r1")]
            [TestCase("brainpoolP320t1")]
            public void ShouldIndicateNonSupportedCurveForSsh(string curve)
            {
                var exception = Assert.Throws<ArgumentException>(() => Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", curve, "-t", "openssh", "--privatekey", "private.pem", "--publickey", "public.openssh"}));
                Assert.AreEqual($"Curve {curve} is not supported for SSH key.", exception.Message);
            }
        }

        [TestFixture]
        public class CreateKeyPair : CreateEcKeyTest
        {
            [TestFixture]
            public class Pkcs8Pem : CreateKeyPair
            {
                [Test]
                public void ShouldWritePkcs8PemFormattedPrivateKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                    
                    byte[] fileContent = fileOutput["private.pem"];
                    string content = encoding.GetString(fileContent);
                    Assert.IsTrue(content.Length > 720 && content.Length < 780);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PRIVATE KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PRIVATE KEY-----{Environment.NewLine}"));
                }
    
                [Test]
                public void ShouldWriteX509PemFormattedPublicKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                    
                    byte[] fileContent = fileOutput["public.pem"];
                    string content = encoding.GetString(fileContent);
                    Assert.IsTrue(content.Length > 420 && content.Length < 500);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PUBLIC KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PUBLIC KEY-----{Environment.NewLine}"));
                }
    
                [Test]
                public void ShouldCreateValidPemFormattedKeyPair()
                {
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                    
                    byte[] privateKeyFileContent = fileOutput["private.pem"];
                    byte[] publicKeyFileContent = fileOutput["public.pem"];
    
                    string privateKeyContent = encoding.GetString(privateKeyFileContent);
                    string publicKeyContent = encoding.GetString(publicKeyFileContent);
                    
                    var container = ContainerProvider.GetContainer();
                    var pkcs8FormattingProvider = container.GetInstance<IPemFormattingProvider<IAsymmetricKey>>();
                    var ecKeyProvider = container.GetInstance<IEcKeyProvider>();
    
                    IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent);
                    IAsymmetricKey publicKey = pkcs8FormattingProvider.GetAsDer(publicKeyContent);
    
                    Assert.IsTrue(ecKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }

                [TestFixture]
                public class Encrypted : Pkcs8Pem
                {
                    [Test]
                    public void ShouldCreateValidKeyPair()
                    {
                        Certifier.Main(new[] {"-c", "key", "-k", "ec", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "-e", "pkcs", "-p", "foobar"});
                        
                        byte[] privateKeyFileContent = fileOutput["private.pem"];
                        byte[] publicKeyFileContent = fileOutput["public.pem"];
        
                        string privateKeyContent = encoding.GetString(privateKeyFileContent);
                        string publicKeyContent = encoding.GetString(publicKeyFileContent);
            
                        var container = ContainerProvider.GetContainer();
                        var pkcs8FormattingProvider = container.GetInstance<IPemFormattingProvider<IAsymmetricKey>>();
                        var ecKeyProvider = container.GetInstance<IEcKeyProvider>();
                        var encryptionProvider = container.GetInstance<KeyEncryptionProvider>();
        
                        IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent);
                        IAsymmetricKey publicKey = pkcs8FormattingProvider.GetAsDer(publicKeyContent);
                        IAsymmetricKey decryptedPrivateKey = encryptionProvider.DecryptPrivateKey(privateKey, "foobar");
        
                        Assert.IsTrue(ecKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(decryptedPrivateKey, publicKey)));
                    }
                }
            }

            [TestFixture]
            public class Pkcs8Der : CreateKeyPair
            {
                [Test]
                public void ShouldCreateValidPkcs8DerFormattedKeyPair()
                {
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "-t", "der", "--privatekey", "private.der", "--publickey", "public.der"});
                
                    byte[] privateKeyFileContent = fileOutput["private.der"];
                    byte[] publicKeyFileContent = fileOutput["public.der"];

                    var container = ContainerProvider.GetContainer();
                    var asymmetricKeyProvider = container.GetInstance<IAsymmetricKeyProvider>();
                    var ecKeyProvider = container.GetInstance<IEcKeyProvider>();

                    IAsymmetricKey privateKey = asymmetricKeyProvider.GetPrivateKey(privateKeyFileContent);
                    IAsymmetricKey publicKey = asymmetricKeyProvider.GetPublicKey(publicKeyFileContent);
                
                    Assert.IsTrue(ecKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }
                
                [TestCase("prime239v3")]
                [TestCase("c2pnb368w1")]
                [TestCase("brainpoolP512t1")]
                [TestCase("sect571r1")]
                public void ShouldCreateValidPkcs8KeyPairWithGivenCurve(string curve)
                {
                    fileOutput = new Dictionary<string, byte[]>();
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", curve, "-t", "der", "--privatekey", "private.der", "--publickey", "public.der"});
                
                    byte[] privateKeyFileContent = fileOutput["private.der"];
                    byte[] publicKeyFileContent = fileOutput["public.der"];

                    var container = ContainerProvider.GetContainer();
                    var asymmetricKeyProvider = container.GetInstance<IAsymmetricKeyProvider>();
                    var ecKeyProvider = container.GetInstance<IEcKeyProvider>();

                    IAsymmetricKey privateKey = asymmetricKeyProvider.GetPrivateKey(privateKeyFileContent);
                    IAsymmetricKey publicKey = asymmetricKeyProvider.GetPublicKey(publicKeyFileContent);
                
                    Assert.IsTrue(ecKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }

                [TestFixture]
                public class Encrypted : Pkcs8Der
                {
                    [Test]
                    public void ShouldCreateValidKeyPair()
                    {
                        Certifier.Main(new[] {"-c", "key", "-k", "ec", "-t", "der", "--privatekey", "private.der", "--publickey", "public.der", "-e", "pkcs", "-p", "foobar"});
                
                        byte[] privateKeyFileContent = fileOutput["private.der"];
                        byte[] publicKeyFileContent = fileOutput["public.der"];
                
                        var container = ContainerProvider.GetContainer();
                        var asymmetricKeyProvider = container.GetInstance<IAsymmetricKeyProvider>();
                        var ecKeyProvider = container.GetInstance<IEcKeyProvider>();
                        var encryptionProvider = container.GetInstance<KeyEncryptionProvider>();

                        IAsymmetricKey encryptedPrivateKey = asymmetricKeyProvider.GetEncryptedPrivateKey(privateKeyFileContent);
                        IAsymmetricKey privateKey = encryptionProvider.DecryptPrivateKey(encryptedPrivateKey, "foobar");
                        IAsymmetricKey publicKey = asymmetricKeyProvider.GetPublicKey(publicKeyFileContent);
                
                        Assert.IsTrue(ecKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                    }                    
                }
            }

            [TestFixture]
            public class OpenSsh : CreateKeyPair
            {
                [Test]
                public void ShouldWritePkcs8PemFormattedPrivateKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", "P-256", "-t", "openssh", "--privatekey", "private.pem", "--publickey", "public.openssh"});
                
                    byte[] fileContent = fileOutput["private.pem"];
                    string content = encoding.GetString(fileContent);

                    Assert.IsTrue(content.Length > 150 && content.Length < 170);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PRIVATE KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PRIVATE KEY-----{Environment.NewLine}"));
                }

                [Test]
                public void ShouldWriteOpenSshPrivateKeyWhenKeyIsCurve25519()
                {
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", "curve25519", "-t", "openssh", "--privatekey", "private.pem", "--publickey", "public.openssh"});
                
                    byte[] fileContent = fileOutput["private.pem"];
                    string content = encoding.GetString(fileContent);

                    Assert.IsTrue(content.Length == 399);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN OPENSSH PRIVATE KEY-----\n"));
                    Assert.IsTrue(content.EndsWith($"-----END OPENSSH PRIVATE KEY-----\n"));
                }

                [TestCase("curve25519", ExpectedResult = "ssh-ed25519")]
                [TestCase("P-256", ExpectedResult = "ecdsa-sha2-nistp256")]
                [TestCase("secp256r1", ExpectedResult = "ecdsa-sha2-nistp256")]
                [TestCase("prime256v1", ExpectedResult = "ecdsa-sha2-nistp256")]
                [TestCase("P-384", ExpectedResult = "ecdsa-sha2-nistp384")]
                [TestCase("secp384r1", ExpectedResult = "ecdsa-sha2-nistp384")]
                [TestCase("P-521", ExpectedResult = "ecdsa-sha2-nistp521")]
                [TestCase("secp521r1", ExpectedResult = "ecdsa-sha2-nistp521")]
                public string ShouldWriteOpenSshFormattedPublicKey(string curve)
                {
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", curve, "-t", "openssh", "--privatekey", "private.pem", "--publickey", "public.openssh"});
                
                    byte[] fileContent = fileOutput["public.openssh"];
                    string content = encoding.GetString(fileContent);
                    string[] splitContent = content.Split((' '));

                    return splitContent[0];
                }

                [Test]
                public void ShouldCreateValidKeyPair()
                {
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "-t", "openssh", "--curve", "P-256", "--privatekey", "private.pem", "--publickey", "public.openssh"});
                    
                    byte[] fileContent = fileOutput["public.openssh"];
                    string content = encoding.GetString(fileContent);
                    
                    var container = ContainerProvider.GetContainer();
                    var sshFormattingProvider = container.GetInstance<ISshFormattingProvider>();
                    var ecKeyProvider = container.GetInstance<IEcKeyProvider>();
                    var pemFormattingProvider = container.GetInstance<IPemFormattingProvider<IAsymmetricKey>>();
                    
                    string privateKeyContent = encoding.GetString(fileOutput["private.pem"]);
                    IAsymmetricKey privateKey = pemFormattingProvider.GetAsDer(privateKeyContent); 
                    IAsymmetricKey publicKey = sshFormattingProvider.GetAsDer(content);

                    Assert.IsTrue(ecKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }                
            }

            [TestFixture]
            public class Ssh2 : CreateKeyPair
            {
                [Test]
                public void ShouldWritePkcs8PemFormattedPrivateKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", "P-256", "-t", "ssh2", "--privatekey", "private.pem", "--publickey", "public.ssh2"});
                
                    byte[] fileContent = fileOutput["private.pem"];
                    string content = encoding.GetString(fileContent);
                    
                    Assert.IsTrue(content.Length > 150 && content.Length < 170);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PRIVATE KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PRIVATE KEY-----{Environment.NewLine}"));
                }

                [Test]
                public void ShouldWriteSsh2FormattedPublicKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", "P-256", "-t", "ssh2", "--privatekey", "private.pem", "--publickey", "public.ssh2"});
                
                    byte[] fileContent = fileOutput["public.ssh2"];
                    string content = encoding.GetString(fileContent);
                    
                    Assert.IsTrue(content.StartsWith($"---- BEGIN SSH2 PUBLIC KEY ----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"---- END SSH2 PUBLIC KEY ----"));
                }
                
                [Test]
                public void ShouldThrowWhenCurve25519IsFormattedAsSsh2()
                {
                    Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", "curve25519", "-t", "ssh2", "--privatekey", "private.pem", "--publickey", "public.openssh"}));
                }
                
                [TestCase("P-256")]
                [TestCase("secp256r1")]
                [TestCase("prime256v1")]
                [TestCase("P-384")]
                [TestCase("secp384r1")]
                [TestCase("P-521")]
                [TestCase("secp521r1")]
                public void ShouldCreateValidKeyPair(string curve)
                {
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", curve, "-t", "ssh2", "--privatekey", "private.pem", "--publickey", "public.ssh2"});
                    
                    byte[] fileContent = fileOutput["public.ssh2"];
                    string content = encoding.GetString(fileContent);
                    
                    var container = ContainerProvider.GetContainer();
                    var sshFormattingProvider = container.GetInstance<ISshFormattingProvider>();
                    var ecKeyProvider = container.GetInstance<IEcKeyProvider>();
                    var pemFormattingProvider = container.GetInstance<IPemFormattingProvider<IAsymmetricKey>>();
                    
                    string privateKeyContent = encoding.GetString(fileOutput["private.pem"]);
                    IAsymmetricKey privateKey = pemFormattingProvider.GetAsDer(privateKeyContent); 
                    IAsymmetricKey publicKey = sshFormattingProvider.GetAsDer(content);

                    Assert.IsTrue(ecKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }
            }

            [TestFixture]
            public class Sec1 : CreateKeyPair
            {
                [Test]
                public void ShouldWriteSec1PemFormattedPrivateKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", "prime256v1", "-t", "sec1", "--privatekey", "private.sec1", "--publickey", "public.pem"});
                
                    byte[] fileContent = fileOutput["private.sec1"];
                    string content = encoding.GetString(fileContent);
                
                    Assert.IsTrue(content.Length > 120 && content.Length < 150);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN EC PRIVATE KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END EC PRIVATE KEY-----{Environment.NewLine}"));
                }

                [Test]
                public void ShouldWriteX509PemFormattedPublicKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", "prime256v1", "-t", "sec1", "--privatekey", "private.pem", "--publickey", "public.pem"});
                    
                    byte[] fileContent = fileOutput["public.pem"];
                    string content = encoding.GetString(fileContent);
                    
                    Assert.IsTrue(content.Length > 160 && content.Length < 190);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PUBLIC KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PUBLIC KEY-----{Environment.NewLine}"));
                }

                [Test]
                public void ShouldCreateValidKeyPair()
                {
                    Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", "prime256v1", "-t", "sec1", "--privatekey", "private.sec1", "--publickey", "public.pem"});
                    
                    var container = ContainerProvider.GetContainer();
                    var ecKeyProvider = container.GetInstance<IEcKeyProvider>();
                    var pemFormattingProvider = container.GetInstance<IPemFormattingProvider<IAsymmetricKey>>();
                    var ecPemFormattingProvider = container.GetInstance<IPemFormattingProvider<IEcKey>>();
                    
                    string privateKeyContent = encoding.GetString(fileOutput["private.sec1"]);
                    string publicKeyContent = encoding.GetString(fileOutput["public.pem"]);
                    
                    IAsymmetricKey privateKey = ecPemFormattingProvider.GetAsDer(privateKeyContent); 
                    IAsymmetricKey publicKey = pemFormattingProvider.GetAsDer(publicKeyContent);

                    Assert.IsTrue(ecKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }

                [Test]
                public void ShouldThrowExceptionWhenNonEcKeyIsSec1Formatted()
                {
                    var exception = Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"-c", "key", "-k", "elgamal", "--fast", "-t", "sec1", "--privatekey", "private.sec1", "--publickey", "public.pem"}));
                    Assert.AreEqual("Only EC keys can be formatted in SECG SEC 1 compatible format.", exception.Message);
                }

                [Test]
                public void ShouldThrowExceptionWhenSec1FormattedKeyIsEncrypted()
                {
                    var exception = Assert.Throws<InvalidOperationException>(() => Certifier.Main(new[] {"-c", "key", "-k", "ec", "--curve", "prime256v1", "-t", "sec1", "--privatekey", "private.sec1", "--publickey", "public.pem", "-e", "pkcs", "-p", "foobar"}));
                    Assert.AreEqual("Encryption of SEC 1 formatted EC keys is not supported.", exception.Message);
                }
            }
        }
    }
}