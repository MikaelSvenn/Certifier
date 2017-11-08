using System;
using System.Collections.Generic;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Crypto.Providers;
using Moq;
using NUnit.Framework;
using SimpleInjector;
using Ui.Console;
using Ui.Console.Provider;

namespace Integration.CreateKey.Test
{
    [TestFixture]
    public class CreateRsaKeyTest
    {
        private Dictionary<string, byte[]> fileOutput;
        private Mock<FileWrapper> file;
        private EncodingWrapper encoding;

        [SetUp]
        public void SetupCreateRsaKeyTest()
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
        public void TeardownCreateRsaKeyTest()
        {
            ContainerProvider.ClearContainer();
        }

        [TestFixture]
        public class ErrorCases : CreateRsaKeyTest
        {
            [Test]
            public void ShouldNotAllowKeySizeBelow2048Bits()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "1024", "--privatekey", "private.pem", "--publickey", "public.pem"}); });
                Assert.AreEqual("RSA key size too small. At least 2048 bit keys are required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPrivateKey()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "2048", "--publickey", "public.pem"}); });
                Assert.AreEqual("Private key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPublicKey()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "2048", "--privatekey", "private.pem"}); });
                Assert.AreEqual("Public key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPassword()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "2048", "-e", "pkcs", "--privatekey", "private.pem", "--publickey", "public.pem"}); });
                Assert.AreEqual("Password is required for encryption.", exception.Message);
            }
        }

        [TestFixture]
        public class CreateKeyPair : CreateRsaKeyTest
        {

            [TestFixture]
            public class Pkcs8Pem : CreateKeyPair
            {
                [Test]
                public void ShouldWritePkcs8PemFormattedPrivateKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                
                    byte[] fileContent = fileOutput["private.pem"];
                    string content = encoding.GetString(fileContent);
                
                    Assert.IsTrue(content.Length > 1600 && content.Length < 1800);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PRIVATE KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PRIVATE KEY-----{Environment.NewLine}"));
                }
                
                [Test]
                public void ShouldWritePkcs8PemFormattedPublicKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                
                    byte[] fileContent = fileOutput["public.pem"];
                    string content = encoding.GetString(fileContent);
                
                    Assert.IsTrue(content.Length > 400 && content.Length < 500);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PUBLIC KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PUBLIC KEY-----{Environment.NewLine}"));
                }

                [Test]
                public void ShouldCreateValidPkcs8PemFormattedKeyPair()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                
                    byte[] privateKeyFileContent = fileOutput["private.pem"];
                    byte[] publicKeyFileContent = fileOutput["public.pem"];

                    string privateKeyContent = encoding.GetString(privateKeyFileContent);
                    string publicKeyContent = encoding.GetString(publicKeyFileContent);
                
                    var container = ContainerProvider.GetContainer();
                    var pkcs8FormattingProvider = container.GetInstance<IPkcsFormattingProvider<IAsymmetricKey>>();
                    var rsaKeyProvider = container.GetInstance<RsaKeyProvider>();

                    IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent);
                    IAsymmetricKey publicKey = pkcs8FormattingProvider.GetAsDer(publicKeyContent);

                    Assert.IsTrue(rsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }

                [TestFixture]
                public class Encrypted : Pkcs8Pem
                {
                    [Test]
                    public void ShouldWritePkcs8PemFormattedEncryptedPrivateKey()
                    {
                        Certifier.Main(new[] {"-c", "key", "-b", "2048", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "-e", "pkcs", "-p", "foobar"});
                        
                        byte[] fileContent = fileOutput["private.pem"];
                        string content = encoding.GetString(fileContent);
                        
                        Assert.IsTrue(content.Length > 3100 && content.Length < 3300);
                        Assert.IsTrue(content.StartsWith($"-----BEGIN ENCRYPTED PRIVATE KEY-----{Environment.NewLine}"));
                        Assert.IsTrue(content.EndsWith($"-----END ENCRYPTED PRIVATE KEY-----{Environment.NewLine}"));
                    }
        
                    [Test]
                    public void ShouldWritePkcs8PemFormattedPublicKeyToGivenFile()
                    {
                        Certifier.Main(new[] {"-c", "key", "-b", "2048", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "-e", "pkcs", "-p", "foobar"});
                        
                        byte[] fileContent = fileOutput["public.pem"];
                        string content = encoding.GetString(fileContent);
                        
                        Assert.IsTrue(content.Length > 400 && content.Length < 500);
                        Assert.IsTrue(content.StartsWith($"-----BEGIN PUBLIC KEY-----{Environment.NewLine}"));
                        Assert.IsTrue(content.EndsWith($"-----END PUBLIC KEY-----{Environment.NewLine}"));
                    }
        
                    [Test]
                    public void ShouldCreateValidKeyPair()
                    {
                        Certifier.Main(new[] {"-c", "key", "-b", "2048", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "-e", "pkcs", "-p", "foobar"});
                        
                        byte[] privateKeyFileContent = fileOutput["private.pem"];
                        byte[] publicKeyFileContent = fileOutput["public.pem"];
        
                        string privateKeyContent = encoding.GetString(privateKeyFileContent);
                        string publicKeyContent = encoding.GetString(publicKeyFileContent);
            
                        var container = ContainerProvider.GetContainer();
                        var pkcs8FormattingProvider = container.GetInstance<IPkcsFormattingProvider<IAsymmetricKey>>();
                        var rsaKeyProvider = container.GetInstance<RsaKeyProvider>();
                        var encryptionProvider = container.GetInstance<PkcsEncryptionProvider>();
        
                        IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent);
                        IAsymmetricKey publicKey = pkcs8FormattingProvider.GetAsDer(publicKeyContent);
                        IAsymmetricKey decryptedPrivateKey = encryptionProvider.DecryptPrivateKey(privateKey, "foobar");
        
                        Assert.IsTrue(rsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(decryptedPrivateKey, publicKey)));
                    }
                }
            }

            [TestFixture]
            public class Pkcs8Der : CreateKeyPair
            {
                [Test]
                public void ShouldCreateValidPkcs8DerFormattedKeyPair()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "--privatekey", "private.der", "--publickey", "public.der", "-t", "der"});
                
                    byte[] privateKeyFileContent = fileOutput["private.der"];
                    byte[] publicKeyFileContent = fileOutput["public.der"];

                    var container = ContainerProvider.GetContainer();
                    var asymmetricKeyProvider = container.GetInstance<IAsymmetricKeyProvider>();
                    var rsaKeyProvider = container.GetInstance<IKeyProvider<RsaKey>>();

                    IAsymmetricKey privateKey = asymmetricKeyProvider.GetPrivateKey(privateKeyFileContent);
                    IAsymmetricKey publicKey = asymmetricKeyProvider.GetPublicKey(publicKeyFileContent);
                
                    Assert.IsTrue(rsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }

                [TestFixture]
                public class Encrypted : Pkcs8Der
                {
                    [Test]
                    public void ShouldCreateValidKeyPair()
                    {
                        Certifier.Main(new[] {"-c", "key", "-b", "2048", "--privatekey", "private.der", "--publickey", "public.der", "-e", "pkcs", "-p", "foobar", "-t", "der"});
                
                        byte[] privateKeyFileContent = fileOutput["private.der"];
                        byte[] publicKeyFileContent = fileOutput["public.der"];
                
                        var container = ContainerProvider.GetContainer();
                        var asymmetricKeyProvider = container.GetInstance<IAsymmetricKeyProvider>();
                        var rsaKeyProvider = container.GetInstance<IKeyProvider<RsaKey>>();
                        var encryptionProvider = container.GetInstance<PkcsEncryptionProvider>();

                        IAsymmetricKey encryptedPrivateKey = asymmetricKeyProvider.GetEncryptedPrivateKey(privateKeyFileContent);
                        IAsymmetricKey privateKey = encryptionProvider.DecryptPrivateKey(encryptedPrivateKey, "foobar");
                        IAsymmetricKey publicKey = asymmetricKeyProvider.GetPublicKey(publicKeyFileContent);
                
                        Assert.IsTrue(rsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                    }
                }
            }

            [TestFixture]
            public class OpenSSh : CreateKeyPair
            {
                [Test]
                public void ShouldWritePkcs8PemFormattedPrivateKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-t", "openssh", "--privatekey", "private.pem", "--publickey", "public.openssh"});
                
                    byte[] fileContent = fileOutput["private.pem"];
                    string content = encoding.GetString(fileContent);
                
                    Assert.IsTrue(content.Length > 1600 && content.Length < 1800);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PRIVATE KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PRIVATE KEY-----{Environment.NewLine}"));
                }

                [Test]
                public void ShouldWriteOpenSshFormattedPublicKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-t", "openssh", "--privatekey", "private.pem", "--publickey", "public.openssh"});
                
                    byte[] fileContent = fileOutput["public.openssh"];
                    string content = encoding.GetString(fileContent);
                    string[] splitContent = content.Split((' '));
                    
                    Assert.AreEqual("ssh-rsa", splitContent[0]);
                }

                [Test]
                public void ShouldCreateValidKeyPair()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-t", "openssh", "--privatekey", "private.pem", "--publickey", "public.openssh"});
                    
                    byte[] fileContent = fileOutput["public.openssh"];
                    string content = encoding.GetString(fileContent);
                    string[] splitContent = content.Split((' '));
                    
                    var container = ContainerProvider.GetContainer();
                    var sshKeyProvider = container.GetInstance<ISshKeyProvider>();
                    var rsaKeyProvider = container.GetInstance<RsaKeyProvider>();
                    var pkcs8FormattingProvider = container.GetInstance<IPkcsFormattingProvider<IAsymmetricKey>>();
                    
                    string privateKeyContent = encoding.GetString(fileOutput["private.pem"]);
                    IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent); 
                    IAsymmetricKey publicKey = sshKeyProvider.GetKeyFromSsh(splitContent[1]);
                     
                    Assert.IsTrue(rsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }
            }

            [TestFixture]
            public class Ssh2 : CreateKeyPair
            {
                [Test]
                public void ShouldWritePkcs8PemFormattedPrivateKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-t", "ssh2", "--privatekey", "private.pem", "--publickey", "public.ssh2"});
                
                    byte[] fileContent = fileOutput["private.pem"];
                    string content = encoding.GetString(fileContent);
                
                    Assert.IsTrue(content.Length > 1600 && content.Length < 1800);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PRIVATE KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PRIVATE KEY-----{Environment.NewLine}"));
                }

                [Test]
                public void ShouldWriteSsh2FormattedPublicKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-t", "ssh2", "--privatekey", "private.pem", "--publickey", "public.ssh2"});
                
                    byte[] fileContent = fileOutput["public.ssh2"];
                    string content = encoding.GetString(fileContent);
                    
                    Assert.IsTrue(content.StartsWith($"---- BEGIN SSH2 PUBLIC KEY ----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"---- END SSH2 PUBLIC KEY ----"));
                }

                [Test]
                public void ShouldCreateValidKeyPair()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-t", "ssh2", "--privatekey", "private.pem", "--publickey", "public.ssh2"});
                    
                    byte[] fileContent = fileOutput["public.ssh2"];
                    string content = encoding.GetString(fileContent);
                    
                    var container = ContainerProvider.GetContainer();
                    var sshFormattingProvider = container.GetInstance<ISshFormattingProvider>();
                    var rsaKeyProvider = container.GetInstance<RsaKeyProvider>();
                    var pkcs8FormattingProvider = container.GetInstance<IPkcsFormattingProvider<IAsymmetricKey>>();
                    
                    string privateKeyContent = encoding.GetString(fileOutput["private.pem"]);
                    IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent); 
                    IAsymmetricKey publicKey = sshFormattingProvider.GetAsDer(content);

                    Assert.IsTrue(rsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }
            }
        }
    }
}