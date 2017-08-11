using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using Crypto.Wrappers;
using Moq;
using NUnit.Framework;
using SimpleInjector;
using Ui.Console.Provider;

namespace Ui.Console.Test.Integration
{
    [TestFixture]
    public abstract class CreateSignatureTest
    {
        private string privateKey;
        private byte[] fileContent;
        private Mock<FileWrapper> file;
        private Mock<ConsoleWrapper> console;
        private Dictionary<string, byte[]> fileOutput;
        private List<string> consoleOutput;
        private EncodingWrapper encoding;
        private IAsymmetricKeyPair keyPair;

        private RsaKeyProvider rsaKeyProvider;
        private DsaKeyProvider dsaKeyProvider;
        private Pkcs8FormattingProvider pkcs8Formatter;
        
        [SetUp]
        public void SetupCreateSignatureTest()
        {
            encoding = new EncodingWrapper();
            var random = new SecureRandomGenerator();
            fileContent = random.NextBytes(10000);

            file = new Mock<FileWrapper>();
            fileOutput = new Dictionary<string, byte[]>();
            file.Setup(f => f.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((path, content) =>
                {
                    fileOutput.Add(path, content);
                });

            file.Setup(f => f.ReadAllBytes("foo.file"))
                .Returns(fileContent);
            
            console = new Mock<ConsoleWrapper>();
            consoleOutput = new List<string>();
            
            console.Setup(c => c.WriteLine(It.IsAny<string>()))
                   .Callback<string>(input => consoleOutput.Add(input));
            
            Container container = ContainerProvider.GetContainer();
            container.Register<FileWrapper>(() => file.Object);
            container.Register<ConsoleWrapper>(() => console.Object);
            
            var asymmetricKeyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            rsaKeyProvider = new RsaKeyProvider(asymmetricKeyPairGenerator);
            dsaKeyProvider = new DsaKeyProvider(asymmetricKeyPairGenerator);
            
            var asymmetricKeyProvider = new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), rsaKeyProvider, dsaKeyProvider);
            pkcs8Formatter = new Pkcs8FormattingProvider(asymmetricKeyProvider);
        }

        [TearDown]
        public void TeardownCreateSignatureTest()
        {
            ContainerProvider.ClearContainer();
        }

        protected void SetupWithRsaKey()
        {
            keyPair = rsaKeyProvider.CreateKeyPair(2048);
            var privateRsaKey = keyPair.PrivateKey;
            privateKey = pkcs8Formatter.GetAsPem(privateRsaKey);

            file.Setup(f => f.ReadAllBytes("private.pem"))
                .Returns(encoding.GetBytes(privateKey));
        }

        protected void SetupWithDsaKey()
        {
            keyPair = dsaKeyProvider.CreateKeyPair(2048);
            var privateDsaKey = keyPair.PrivateKey;
            privateKey = pkcs8Formatter.GetAsPem(privateDsaKey);

            file.Setup(f => f.ReadAllBytes("private.pem"))
                .Returns(encoding.GetBytes(privateKey));
        }
        
        public void VerifySignature(string signatureFileName = "", string content = "")
        {
            Container container = ContainerProvider.GetContainer();
            var signatureProvider = container.GetInstance<SignatureProvider>();

            byte[] encodedSignatureFile = string.IsNullOrEmpty(signatureFileName) ? encoding.GetBytes(consoleOutput.Single()) : fileOutput[signatureFileName];
            string encodedSignature = encoding.GetString(encodedSignatureFile);
            byte[] signedData = string.IsNullOrEmpty(content) ? fileContent : encoding.GetBytes(content);
            
            var signature = new Signature
            {
                Content = Convert.FromBase64String(encodedSignature),
                SignedData = signedData 
            };
            
            Assert.IsTrue(signatureProvider.VerifySignature(keyPair.PublicKey, signature));
        }

        [TestFixture]
        public class CreateRsaSignature : CreateSignatureTest
        {
            [SetUp]
            public void SetupCreateRsaSignature()
            {
                SetupWithRsaKey();
            }
        }

        [TestFixture]
        public class CreateDsaSignature : CreateSignatureTest
        {
            [SetUp]
            public void SetupCreateDsaSignature()
            {
                SetupWithDsaKey();
            }
        }

        [Test]
        public void ShouldWriteBase64EncodedSignatureToFile()
        {
            Certifier.Main(new[] {"-c", "signature", "--privatekey", "private.pem", "-i", "foobarbaz", "-o", "foo.file.signature"});

            byte[] signatureContent = fileOutput["foo.file.signature"];
            string signature = encoding.GetString(signatureContent);

            Assert.IsNotEmpty(signature);
            Assert.DoesNotThrow(() => Convert.FromBase64String(signature));
        }

        [Test]
        public void ShouldWriteBase64EncodedSignatureToStdoutWhenOutputIsNotSpecified()
        {
            Certifier.Main(new[] {"-c", "signature", "--privatekey", "private.pem", "-i", "foobarbaz"});
            string signature = consoleOutput.Single();
            
            Assert.IsNotEmpty(signature);
            Assert.DoesNotThrow(() => { Convert.FromBase64String(signature); });
        }

        [Test]
        public void ShouldCreateValidSignatureForFileInputAndFileOutput()
        {
            Certifier.Main(new[] {"-c", "signature", "--privatekey", "private.pem", "-f", "foo.file", "-o", "foo.file.signature"});
            VerifySignature("foo.file.signature");
        }

        [Test]
        public void ShouldCreateValidSignatureForStandardInputAndFileOutput()
        {
            const string input = @"FooBarBaz©®*Åäö!      _<>#|?€  \n\t  I said this. It is guaranteed.";  
            Certifier.Main(new[] {"-c", "signature", "--privatekey", "private.pem", "-i", input, "-o", "bar.file.signature"});
            VerifySignature("bar.file.signature", input);
        }

        [Test]
        public void ShouldCreateValidSignatureForFileInputAndConsoleOutput()
        {
            Certifier.Main(new[] {"-c", "signature", "--privatekey", "private.pem", "-f", "foo.file"});
            VerifySignature();
        }

        [Test]
        public void ShouldCreateValidSignatureForStandardInputAndConsoleOutput()
        {
            const string input = "It's me, Mario!";
            Certifier.Main(new[] {"-c", "signature", "--privatekey", "private.pem", "-i", input});
            VerifySignature("", input);
        }

        [Test]
        public void ShouldIndicateMissingPrivateKey()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                Certifier.Main(new []{"-c", "signature", "-i", "foobarbaz"});
            });

            Assert.AreEqual("Private key file or path is required.", exception.Message);
        }
    }
}