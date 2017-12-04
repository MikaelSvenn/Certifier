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
    public class CreateElgamalKeyTest
    {
        private Dictionary<string, byte[]> fileOutput;
        private Mock<FileWrapper> file;
        private EncodingWrapper encoding;

        [SetUp]
        public void SetupCreateElgamalKey()
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
        public class ErrorCases : CreateElgamalKeyTest
        {
            [Test]
            public void ShouldNotAllowIncorrectKeySize()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "1500", "-k", "elgamal", "--privatekey", "private.pem", "--publickey", "public.pem", "--fast"}); });
                Assert.AreEqual("ElGamal key size can either be 2048, 3072, 4096, 6144 or 8192 bits.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPrivateKey()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "elgamal", "--publickey", "public.pem", "--fast"}); });
                Assert.AreEqual("Private key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPublicKey()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "elgamal", "--privatekey", "private.pem", "--fast"}); });
                Assert.AreEqual("Public key file or path is required.", exception.Message);
            }

            [Test]
            public void ShouldIndicateMissingPassword()
            {
                var exception = Assert.Throws<ArgumentException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "elgamal", "-e", "pkcs", "--privatekey", "private.pem", "--publickey", "public.pem", "--fast"}); });
                Assert.AreEqual("Password is required for encryption.", exception.Message);
            }

            [Test]
            public void ShouldIndicateOpenSshIncompatibility()
            {
                var exception = Assert.Throws<InvalidOperationException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "elgamal", "-t", "openssh", "--privatekey", "private.pem", "--publickey", "public.pem", "--fast"}); });
                Assert.AreEqual("Cipher type not supported for OpenSSH key.", exception.Message);
            }

            [Test]
            public void ShouldIndicateSsh2Incompatibility()
            {
                var exception = Assert.Throws<InvalidOperationException>(() => { Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "elgamal", "-t", "ssh2", "--privatekey", "private.pem", "--publickey", "public.pem", "--fast"}); });
                Assert.AreEqual("Cipher type not supported for SSH2 key.", exception.Message);
            }
        }

        [TestFixture]
        public class CreateKeyPairTest : CreateElgamalKeyTest
        {
            [TestFixture]
            public class Pkcs8Pem : CreateKeyPairTest
            {
                [Test]
                public void ShouldWritePkcs8PemFormattedPrivateKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "elgamal", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "--fast"});
                    
                    byte[] fileContent = fileOutput["private.pem"];
                    string content = encoding.GetString(fileContent);
    
                    Assert.IsTrue(content.Length > 800 && content.Length < 850);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PRIVATE KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PRIVATE KEY-----{Environment.NewLine}"));
                }
    
                [Test]
                public void ShouldWritePkcs8PemFormattedPublicKey()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "elgamal", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "--fast"});
                    
                    byte[] fileContent = fileOutput["public.pem"];
                    string content = encoding.GetString(fileContent);
                    
                    Assert.IsTrue(content.Length > 800 && content.Length < 850);
                    Assert.IsTrue(content.StartsWith($"-----BEGIN PUBLIC KEY-----{Environment.NewLine}"));
                    Assert.IsTrue(content.EndsWith($"-----END PUBLIC KEY-----{Environment.NewLine}"));
                }
    
                [Test]
                public void ShouldCreateValidPkcs8PemFormattedKeyPair()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "elgamal", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "--fast"});
                    
                    byte[] privateKeyFileContent = fileOutput["private.pem"];
                    byte[] publicKeyFileContent = fileOutput["public.pem"];
    
                    string privateKeyContent = encoding.GetString(privateKeyFileContent);
                    string publicKeyContent = encoding.GetString(publicKeyFileContent);
                    
                    var container = ContainerProvider.GetContainer();
                    var pkcs8FormattingProvider = container.GetInstance<IPemFormattingProvider<IAsymmetricKey>>();
                    var elgamalKeyProvider = container.GetInstance<IElGamalKeyProvider>();
    
                    IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent);
                    IAsymmetricKey publicKey = pkcs8FormattingProvider.GetAsDer(publicKeyContent);
    
                    Assert.IsTrue(elgamalKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }

                [TestFixture]
                public class Encrypted : Pkcs8Pem
                {
                    [Test]
                    public void ShouldWritePkcs8PemFormattedEncryptedPrivateKey()
                    {
                        Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "elgamal", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "-e", "pkcs", "-p", "foobar", "--fast"});
                        
                        byte[] fileContent = fileOutput["private.pem"];
                        string content = encoding.GetString(fileContent);
                        
                        Assert.IsTrue(content.Length > 2200 && content.Length < 2400);
                        Assert.IsTrue(content.StartsWith($"-----BEGIN ENCRYPTED PRIVATE KEY-----{Environment.NewLine}"));
                        Assert.IsTrue(content.EndsWith($"-----END ENCRYPTED PRIVATE KEY-----{Environment.NewLine}"));
                    }
        
                    [Test]
                    public void ShouldWritePemFormattedPublicKey()
                    {
                        Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "elgamal", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "-e", "pkcs", "-p", "foobar", "--fast"});
                        
                        byte[] fileContent = fileOutput["public.pem"];
                        string content = encoding.GetString(fileContent);
                        
                        Assert.IsTrue(content.Length > 800 && content.Length < 850);
                        Assert.IsTrue(content.StartsWith($"-----BEGIN PUBLIC KEY-----{Environment.NewLine}"));
                        Assert.IsTrue(content.EndsWith($"-----END PUBLIC KEY-----{Environment.NewLine}"));
                    }
        
                    [Test]
                    public void ShouldCreateValidEncryptedPkcs8PemFormattedKeyPair()
                    {
                        Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "elgamal", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "-e", "pkcs", "-p", "foobar", "--fast"});
                        
                        byte[] privateKeyFileContent = fileOutput["private.pem"];
                        byte[] publicKeyFileContent = fileOutput["public.pem"];
        
                        string privateKeyContent = encoding.GetString(privateKeyFileContent);
                        string publicKeyContent = encoding.GetString(publicKeyFileContent);
            
                        var container = ContainerProvider.GetContainer();
                        var pkcs8FormattingProvider = container.GetInstance<IPemFormattingProvider<IAsymmetricKey>>();
                        var elgamalKeyProvider = container.GetInstance<IElGamalKeyProvider>();
                        var encryptionProvider = container.GetInstance<Pkcs8EncryptionProvider>();
        
                        IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent);
                        IAsymmetricKey publicKey = pkcs8FormattingProvider.GetAsDer(publicKeyContent);
                        IAsymmetricKey decryptedPrivateKey = encryptionProvider.DecryptPrivateKey(privateKey, "foobar");
        
                        Assert.IsTrue(elgamalKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(decryptedPrivateKey, publicKey)));
                    }
                }
            }

            [TestFixture]
            public class Pkcs8Der : CreateKeyPairTest
            {
                [Test]
                public void ShouldCreateValidPkcs8DerFormattedKeyPair()
                {
                    Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "elgamal", "--privatekey", "private.der", "--publickey", "public.der", "--fast"});
                
                    byte[] privateKeyFileContent = fileOutput["private.der"];
                    byte[] publicKeyFileContent = fileOutput["public.der"];

                    var container = ContainerProvider.GetContainer();
                    var asymmetricKeyProvider = container.GetInstance<IAsymmetricKeyProvider>();
                    var elgamalKeyProvider = container.GetInstance<IElGamalKeyProvider>();

                    IAsymmetricKey privateKey = asymmetricKeyProvider.GetPrivateKey(privateKeyFileContent);
                    IAsymmetricKey publicKey = asymmetricKeyProvider.GetPublicKey(publicKeyFileContent);
                
                    Assert.IsTrue(elgamalKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                }

                [TestFixture]
                public class Encrypted : Pkcs8Der
                {
                    [Test]
                    public void ShouldCreateValidEncryptedPkcs8DerFormattedKeyPair()
                    {
                        Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "elgamal", "--privatekey", "private.der", "--publickey", "public.der", "-e", "pkcs", "-p", "foobar", "-t", "der", "--fast"});
                
                        byte[] privateKeyFileContent = fileOutput["private.der"];
                        byte[] publicKeyFileContent = fileOutput["public.der"];
                
                        var container = ContainerProvider.GetContainer();
                        var asymmetricKeyProvider = container.GetInstance<IAsymmetricKeyProvider>();
                        var elgamalKeyProvider = container.GetInstance<IElGamalKeyProvider>();
                        var encryptionProvider = container.GetInstance<Pkcs8EncryptionProvider>();

                        IAsymmetricKey encryptedPrivateKey = asymmetricKeyProvider.GetEncryptedPrivateKey(privateKeyFileContent);
                        IAsymmetricKey privateKey = encryptionProvider.DecryptPrivateKey(encryptedPrivateKey, "foobar");
                        IAsymmetricKey publicKey = asymmetricKeyProvider.GetPublicKey(publicKeyFileContent);
                
                        Assert.IsTrue(elgamalKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
                    }
                }
            }
        }
    }
}