using System;
using System.Collections.Generic;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Crypto.Providers;
using Moq;
using NUnit.Framework;
using SimpleInjector;
using Ui.Console.Provider;

namespace Ui.Console.Test.Integration
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
            [Test]
            public void ShouldWritePkcs8FormattedPrivateKeyToGivenFile()
            {
                Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                
                byte[] fileContent = fileOutput["private.pem"];
                string content = encoding.GetString(fileContent);

                Assert.IsTrue(content.Length > 850 && content.Length < 1000);
                Assert.IsTrue(content.StartsWith($"-----BEGIN PRIVATE KEY-----{Environment.NewLine}"));
                Assert.IsTrue(content.EndsWith($"-----END PRIVATE KEY-----{Environment.NewLine}"));
            }

            [Test]
            public void ShouldWritePkcs8FormattedPublicKeyToGivenFile()
            {
                Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                
                byte[] fileContent = fileOutput["public.pem"];
                string content = encoding.GetString(fileContent);

                Assert.IsTrue(content.Length > 1100 && content.Length < 1300);
                Assert.IsTrue(content.StartsWith($"-----BEGIN PUBLIC KEY-----{Environment.NewLine}"));
                Assert.IsTrue(content.EndsWith($"-----END PUBLIC KEY-----{Environment.NewLine}"));
            }

            [Test]
            public void ShouldCreateValidPemFormattedKeyPair()
            {
                Certifier.Main(new[] {"-c", "key", "-b", "2048","-k", "dsa", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem"});
                
                byte[] privateKeyFileContent = fileOutput["private.pem"];
                byte[] publicKeyFileContent = fileOutput["public.pem"];

                string privateKeyContent = encoding.GetString(privateKeyFileContent);
                string publicKeyContent = encoding.GetString(publicKeyFileContent);
                
                var container = ContainerProvider.GetContainer();
                var pkcs8FormattingProvider = container.GetInstance<IPkcsFormattingProvider<IAsymmetricKey>>();
                var dsaKeyProvider = container.GetInstance<DsaKeyProvider>();

                IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent);
                IAsymmetricKey publicKey = pkcs8FormattingProvider.GetAsDer(publicKeyContent);

                Assert.IsTrue(dsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
            }

            [Test]
            public void ShouldCreateValidDerFormattedKeyPair()
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
        }
        
        [TestFixture]
        public class CreatePkcsEncryptedKeyPair : CreateDsaKeyTest
        {
            [Test]
            public void ShouldWritePkcs8FormattedEncryptedPrivateKeyToGivenFile()
            {
                Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "-e", "pkcs", "-p", "foobar"});
                
                byte[] fileContent = fileOutput["private.pem"];
                string content = encoding.GetString(fileContent);
                
                Assert.IsTrue(content.Length > 2200 && content.Length < 2400);
                Assert.IsTrue(content.StartsWith($"-----BEGIN ENCRYPTED PRIVATE KEY-----{Environment.NewLine}"));
                Assert.IsTrue(content.EndsWith($"-----END ENCRYPTED PRIVATE KEY-----{Environment.NewLine}"));
            }

            [Test]
            public void ShouldWritePkcs8FormattedPublicKeyToGivenFile()
            {
                Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "-e", "pkcs", "-p", "foobar"});
                
                byte[] fileContent = fileOutput["public.pem"];
                string content = encoding.GetString(fileContent);
                
                Assert.IsTrue(content.Length > 1100 && content.Length < 1300);
                Assert.IsTrue(content.StartsWith($"-----BEGIN PUBLIC KEY-----{Environment.NewLine}"));
                Assert.IsTrue(content.EndsWith($"-----END PUBLIC KEY-----{Environment.NewLine}"));
            }

            [Test]
            public void ShouldCreateValidPemFormattedKeyPair()
            {
                Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "-t", "pem", "--privatekey", "private.pem", "--publickey", "public.pem", "-e", "pkcs", "-p", "foobar"});
                
                byte[] privateKeyFileContent = fileOutput["private.pem"];
                byte[] publicKeyFileContent = fileOutput["public.pem"];

                string privateKeyContent = encoding.GetString(privateKeyFileContent);
                string publicKeyContent = encoding.GetString(publicKeyFileContent);
    
                var container = ContainerProvider.GetContainer();
                var pkcs8FormattingProvider = container.GetInstance<IPkcsFormattingProvider<IAsymmetricKey>>();
                var dsaKeyProvider = container.GetInstance<DsaKeyProvider>();
                var encryptionProvider = container.GetInstance<PkcsEncryptionProvider>();

                IAsymmetricKey privateKey = pkcs8FormattingProvider.GetAsDer(privateKeyContent);
                IAsymmetricKey publicKey = pkcs8FormattingProvider.GetAsDer(publicKeyContent);
                IAsymmetricKey decryptedPrivateKey = encryptionProvider.DecryptPrivateKey(privateKey, "foobar");

                Assert.IsTrue(dsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(decryptedPrivateKey, publicKey)));
            }

            [Test]
            public void ShouldCreateValidDerFormattedKeyPair()
            {
                Certifier.Main(new[] {"-c", "key", "-b", "2048", "-k", "dsa", "--privatekey", "private.der", "--publickey", "public.der", "-e", "pkcs", "-p", "foobar", "-t", "der"});
                
                byte[] privateKeyFileContent = fileOutput["private.der"];
                byte[] publicKeyFileContent = fileOutput["public.der"];
                
                var container = ContainerProvider.GetContainer();
                var asymmetricKeyProvider = container.GetInstance<IAsymmetricKeyProvider>();
                var dsaKeyProvider = container.GetInstance<IKeyProvider<DsaKey>>();
                var encryptionProvider = container.GetInstance<PkcsEncryptionProvider>();

                IAsymmetricKey encryptedPrivateKey = asymmetricKeyProvider.GetEncryptedPrivateKey(privateKeyFileContent);
                IAsymmetricKey privateKey = encryptionProvider.DecryptPrivateKey(encryptedPrivateKey, "foobar");
                IAsymmetricKey publicKey = asymmetricKeyProvider.GetPublicKey(publicKeyFileContent);
                
                Assert.IsTrue(dsaKeyProvider.VerifyKeyPair(new AsymmetricKeyPair(privateKey, publicKey)));
            }
        }
    }
}