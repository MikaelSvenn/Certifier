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
    public class CreateDsaKeyTest
    {
        private Dictionary<string, byte[]> fileOutput;
        private Mock<FileWrapper> file;
        private EncodingWrapper encoding;

        [SetUp]
        public void SetupCreateDsaKeyTest()
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
        public void TeardownCreateDsaKeyTest()
        {
            ContainerProvider.ClearContainer();
        }
        
        [TestFixture]
        public class ErrorCases : CreateDsaKeyTest
        {
            [Test]
            public void ShouldNotAllowKeySizeBelow2048Bits()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "1024", "-k", "dsa", "--privatekey", "private.pem", "--publickey", "public.pem"}); });
                Assert.AreEqual("DSA key size can only be 2048 bit or 3072 bit.", exception.Message);
            }

            [Test]
            public void ShouldNotAllowKeySizeAbove3072Bits()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "4096", "-k", "dsa", "--privatekey", "private.pem", "--publickey", "public.pem"}); });
                Assert.AreEqual("DSA key size can only be 2048 bit or 3072 bit.", exception.Message);
            }
            
            [Test]
            public void ShouldIndicateMissingPrivateKey()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-k", "dsa", "-b", "2048", "--publickey", "public.pem"}); });
                Assert.AreEqual("Private key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPublicKey()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-k", "dsa", "-b", "2048","--privatekey", "private.pem"}); });
                Assert.AreEqual("Public key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPassword()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-k", "dsa", "-b", "2048","-e", "pkcs", "--privatekey", "private.pem", "--publickey", "public.pem"}); });
                Assert.AreEqual("Password is required for encryption.", exception.Message);
            }
        }
        
        
        [TestFixture]
        public class CreateKeyPair : CreateDsaKeyTest
        {
            [TestFixture]
            public class Pkcs8Pem : CreateKeyPair
            {
                [Test]
                public void ShouldWritePkcs8PemFormattedPrivateKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                    
                    byte[] fileContent = fileOutput["private.pem"];
                    string content = encoding.GetString(fileContent);
    
                    Assert.IsTrue(content.Length > 850 && content.Length < 1000);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PRIVATE KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PRIVATE KEY-----{Environment.NewLine}"));
                }
    
                [Test]
                public void ShouldWritePkcs8PemFormattedPublicKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                    
                    byte[] fileContent = fileOutput["public.pem"];
                    string content = encoding.GetString(fileContent);
    
                    Assert.IsTrue(content.Length > 1100 && content.Length < 1300);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PUBLIC KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PUBLIC KEY-----{Environment.NewLine}"));
                }
    
                [Test]
                public void ShouldCreateValidPkcs8PemFormattedKeyPair()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048","-k", "dsa", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                    
                    byte[] privateKeyFileContent = fileOutput["private.pem"];
                    byte[] publicKeyFileContent = fileOutput["public.pem"];
    
                    string privateKeyContent = encoding.GetString(privateKeyFileContent);
                    string publicKeyContent = encoding.GetString(publicKeyFileContent);
                    
                    var container = ContainerProvider.GetContainer();
                    var pkcs8FormattingProvider = container.GetInstance<IPemFormattingProvider<IAsymmetricKey>>();
                    var dsaKeyProvider = container.GetInstance<DsaKeyProvider>();
    
                    IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent);
                    IAsymmetricKey publicKey = pkcs8FormattingProvider.GetAsDer(publicKeyContent);
    
                    Assert.IsTrue(dsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }

                [TestFixture]
                public class Encrypted : Pkcs8Pem
                {
                    [Test]
                    public void ShouldWritePkcs8PemFormattedEncryptedPrivateKey()
                    {
                        Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "-e", "pkcs", "-p", "foobar"});
                
                        byte[] fileContent = fileOutput["private.pem"];
                        string content = encoding.GetString(fileContent);
                
                        Assert.IsTrue(content.Length > 2200 && content.Length < 2400);
                        Assert.IsTrue(content.StartsWith($"-----BEGIN ENCRYPTED PRIVATE KEY-----{Environment.NewLine}"));
                        Assert.IsTrue(content.EndsWith($"-----END ENCRYPTED PRIVATE KEY-----{Environment.NewLine}"));
                    }

                    [Test]
                    public void ShouldWritePkcs8PemPublicKey()
                    {
                        Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "-e", "pkcs", "-p", "foobar"});
                
                        byte[] fileContent = fileOutput["public.pem"];
                        string content = encoding.GetString(fileContent);
                
                        Assert.IsTrue(content.Length > 1100 && content.Length < 1300);
                        Assert.IsTrue(content.StartsWith($"-----BEGIN PUBLIC KEY-----{Environment.NewLine}"));
                        Assert.IsTrue(content.EndsWith($"-----END PUBLIC KEY-----{Environment.NewLine}"));
                    }
                    
                    [Test]
                    public void ShouldCreateValidEncryptedPkcs8PemFormattedKeyPair()
                    {
                        Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "-e", "pkcs", "-p", "foobar"});
                
                        byte[] privateKeyFileContent = fileOutput["private.pem"];
                        byte[] publicKeyFileContent = fileOutput["public.pem"];

                        string privateKeyContent = encoding.GetString(privateKeyFileContent);
                        string publicKeyContent = encoding.GetString(publicKeyFileContent);
    
                        var container = ContainerProvider.GetContainer();
                        var pkcs8FormattingProvider = container.GetInstance<IPemFormattingProvider<IAsymmetricKey>>();
                        var dsaKeyProvider = container.GetInstance<DsaKeyProvider>();
                        var encryptionProvider = container.GetInstance<Pkcs8EncryptionProvider>();

                        IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent);
                        IAsymmetricKey publicKey = pkcs8FormattingProvider.GetAsDer(publicKeyContent);
                        IAsymmetricKey decryptedPrivateKey = encryptionProvider.DecryptPrivateKey(privateKey, "foobar");

                        Assert.IsTrue(dsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(decryptedPrivateKey, publicKey)));
                    }
                }
            }

            [TestFixture]
            public class Pkcs8Der : CreateKeyPair
            {
                [Test]
                public void ShouldCreateValidPkcs8DerFormattedKeyPair()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "--privatekey", "private.der", "--publickey", "public.der", "-t", "der"});
                
                    byte[] privateKeyFileContent = fileOutput["private.der"];
                    byte[] publicKeyFileContent = fileOutput["public.der"];

                    var container = ContainerProvider.GetContainer();
                    var asymmetricKeyProvider = container.GetInstance<IAsymmetricKeyProvider>();
                    var dsaKeyProvider = container.GetInstance<IKeyProvider<DsaKey>>();

                    IAsymmetricKey privateKey = asymmetricKeyProvider.GetPrivateKey(privateKeyFileContent);
                    IAsymmetricKey publicKey = asymmetricKeyProvider.GetPublicKey(publicKeyFileContent);
                
                    Assert.IsTrue(dsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }

                [TestFixture]
                public class Encrypted : Pkcs8Der
                {
                    [Test]
                    public void ShouldCreateValidEncryptedPkcs8DerFormattedKeyPair()
                    {
                        Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "--privatekey", "private.der", "--publickey", "public.der", "-e", "pkcs", "-p", "foobar", "-t", "der"});
                
                        byte[] privateKeyFileContent = fileOutput["private.der"];
                        byte[] publicKeyFileContent = fileOutput["public.der"];
                
                        var container = ContainerProvider.GetContainer();
                        var asymmetricKeyProvider = container.GetInstance<IAsymmetricKeyProvider>();
                        var dsaKeyProvider = container.GetInstance<IKeyProvider<DsaKey>>();
                        var encryptionProvider = container.GetInstance<Pkcs8EncryptionProvider>();

                        IAsymmetricKey encryptedPrivateKey = asymmetricKeyProvider.GetEncryptedPrivateKey(privateKeyFileContent);
                        IAsymmetricKey privateKey = encryptionProvider.DecryptPrivateKey(encryptedPrivateKey, "foobar");
                        IAsymmetricKey publicKey = asymmetricKeyProvider.GetPublicKey(publicKeyFileContent);
                
                        Assert.IsTrue(dsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                    }
                }
            }

            [TestFixture]
            public class OpenSsh : CreateKeyPair
            {
                [Test]
                public void ShouldWritePkcs8PemFormattedPrivateKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "openssh", "--privatekey", "private.pem", "--publickey", "public.openssh"});
                
                    byte[] fileContent = fileOutput["private.pem"];
                    string content = encoding.GetString(fileContent);
                
                    Assert.IsTrue(content.Length > 850 && content.Length < 1000);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PRIVATE KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PRIVATE KEY-----{Environment.NewLine}"));
                }

                [Test]
                public void ShouldWriteOpenSshFormattedPublicKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "openssh", "--privatekey", "private.pem", "--publickey", "public.openssh"});
                
                    byte[] fileContent = fileOutput["public.openssh"];
                    string content = encoding.GetString(fileContent);
                    string[] splitContent = content.Split((' '));
                    
                    Assert.AreEqual("ssh-dss", splitContent[0]);
                }

                [Test]
                public void ShouldCreateValidKeyPair()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "openssh", "--privatekey", "private.pem", "--publickey", "public.openssh"});
                    
                    byte[] fileContent = fileOutput["public.openssh"];
                    string content = encoding.GetString(fileContent);
                    string[] splitContent = content.Split((' '));
                    
                    var container = ContainerProvider.GetContainer();
                    var sshKeyProvider = container.GetInstance<ISshKeyProvider>();
                    var dsaKeyProvider = container.GetInstance<DsaKeyProvider>();
                    var pkcs8FormattingProvider = container.GetInstance<IPemFormattingProvider<IAsymmetricKey>>();
                    
                    string privateKeyContent = encoding.GetString(fileOutput["private.pem"]);
                    IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent); 
                    IAsymmetricKey publicKey = sshKeyProvider.GetKeyFromSsh(splitContent[1]);
                     
                    Assert.IsTrue(dsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }
            }

            [TestFixture]
            public class Ssh2 : CreateKeyPair
            {
                 [Test]
                public void ShouldWritePkcs8PemFormattedPrivateKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "ssh2", "--privatekey", "private.pem", "--publickey", "public.ssh2"});
                
                    byte[] fileContent = fileOutput["private.pem"];
                    string content = encoding.GetString(fileContent);
                
                    Assert.IsTrue(content.Length > 850 && content.Length < 1000);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PRIVATE KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PRIVATE KEY-----{Environment.NewLine}"));
                }

                [Test]
                public void ShouldWriteSsh2FormattedPublicKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "ssh2", "--privatekey", "private.pem", "--publickey", "public.ssh2"});
                
                    byte[] fileContent = fileOutput["public.ssh2"];
                    string content = encoding.GetString(fileContent);
                    
                    Assert.IsTrue(content.StartsWith($"---- BEGIN SSH2 PUBLIC KEY ----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"---- END SSH2 PUBLIC KEY ----"));
                }

                [Test]
                public void ShouldCreateValidKeyPair()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "ssh2", "--privatekey", "private.pem", "--publickey", "public.ssh2"});
                    
                    byte[] fileContent = fileOutput["public.ssh2"];
                    string content = encoding.GetString(fileContent);
                    
                    var container = ContainerProvider.GetContainer();
                    var sshFormattingProvider = container.GetInstance<ISshFormattingProvider>();
                    var dsaKeyProvider = container.GetInstance<DsaKeyProvider>();
                    var pkcs8FormattingProvider = container.GetInstance<IPemFormattingProvider<IAsymmetricKey>>();
                    
                    string privateKeyContent = encoding.GetString(fileOutput["private.pem"]);
                    IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent); 
                    IAsymmetricKey publicKey = sshFormattingProvider.GetAsDer(content);

                    Assert.IsTrue(dsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }
            }
        }
    }
}