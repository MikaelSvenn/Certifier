using System.Collections.Generic;
using System.Security.Cryptography;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Crypto.Generators;
using Crypto.Mappers;
using Crypto.Providers;
using Crypto.Wrappers;
using Moq;
using NUnit.Framework;
using Ui.Console.Provider;
using Container = SimpleInjector.Container;

namespace Ui.Console.Test.Integration
{
    [TestFixture]
    public class VerifySignatureTest
    {
        private Mock<FileWrapper> file;
        private Mock<ConsoleWrapper> console;
        private Dictionary<string, byte[]> files;
        private string base64FileSignature;
        private string base64InputSignature;

        [SetUp]
        public void VerifySignatureTestSetup()
        {
            var random = new SecureRandomGenerator();
            byte[] fileContent = random.NextBytes(1500);
            
            file = new Mock<FileWrapper>();            
            file.Setup(f => f.ReadAllBytes(It.IsAny<string>()))
                .Returns<string>(givenFile => files[givenFile]);

            console = new Mock<ConsoleWrapper>();
            
            Container container = ContainerProvider.GetContainer();
            container.Register<FileWrapper>(() => file.Object);
            container.Register<ConsoleWrapper>(() => console.Object);

            var keyProvider = new RsaKeyProvider(new RsaKeyPairGenerator(new SecureRandomGenerator()));
            var signatureProvider = new SignatureProvider(new SignatureAlgorithmIdentifierMapper(), new SecureRandomGenerator(), new SignerUtilitiesWrapper());
            var pkcs8Formatter = new Pkcs8FormattingProvider(new AsymmetricKeyProvider(new OidToCipherTypeMapper(), keyProvider, new KeyInfoWrapper()));

            IAsymmetricKeyPair keyPair = keyProvider.CreateKeyPair(2048);
            Signature fileSignature = signatureProvider.CreateSignature(keyPair.PrivateKey, fileContent);
            
            var base64 = new Base64Wrapper();
            base64FileSignature = base64.ToBase64String(fileSignature.Content);
            
            var encoding = new EncodingWrapper();
            Signature inputSignature = signatureProvider.CreateSignature(keyPair.PrivateKey, encoding.GetBytes("FooBarBaz"));
            base64InputSignature = base64.ToBase64String(inputSignature.Content);
            
            string pemFormattedPublicKey = pkcs8Formatter.GetAsPem(keyPair.PublicKey);
            
            var tamperedFileContent = (byte[])fileContent.Clone();
            tamperedFileContent[10] = (byte) (tamperedFileContent[10] >> 1);
            
            files = new Dictionary<string, byte[]>
            {
                {"content.file", fileContent}, 
                {"tamperedContent.file", tamperedFileContent},
                {"signature.file", encoding.GetBytes(base64FileSignature)},
                {"signature.input", encoding.GetBytes(base64InputSignature)},
                {"public.pem", encoding.GetBytes(pemFormattedPublicKey)}
            };
        }

        private string TamperBase64Signature(string base64Signature)
        {
            char[] signatureCharacters = base64Signature.ToCharArray();
            signatureCharacters[15] = signatureCharacters[15] == 'A' ? 'a' : 'A';
            return new string(signatureCharacters);
        }
        
        [TearDown]
        public void TeardownCreateSignatureTest()
        {
            ContainerProvider.ClearContainer();
        }
        
        [TestFixture]
        public class VerifyRsaSignature : VerifySignatureTest
        {
            public static string[][] invalidFileSignatures = {
                new[]{"-v", "signature", "--publickey", "public.pem", "-f", "content.file", "-s", "signature.input"}, 
                new[]{"-v", "signature", "--publickey", "public.pem", "-f", "tamperedContent.file", "-s", "signature.file"}, 
                new[]{"-v", "signature", "--publickey", "public.pem", "-i", "FooBarBaz", "-s", "signature.file"}, 
                new[]{"-v", "signature", "--publickey", "public.pem", "-i", "FooBarBaz.", "-s", "signature.input"}
            };
            
            [Test, TestCaseSource("invalidFileSignatures")]
            public void ShouldThrowExceptionForInvalidFileSignature(string[] applicationParameters)
            {
                Assert.Throws<CryptographicException>(() => Certifier.Main(applicationParameters));
            }

            [Test]
            public void ShouldThrowExceptionWhenInvalidSignatureIsGivenAsParameterForFileContent()
            {
                string tamperedBase64FileSignature = TamperBase64Signature(base64FileSignature);
                Assert.Throws<CryptographicException>(() => Certifier.Main(new[]{"-v", "signature", "--publickey", "public.pem", "-f", "content.file", "-s", tamperedBase64FileSignature}));
            }

            [Test]
            public void ShouldThrowExceptionWhenInvalidSignatureIsGivenAsParameterForUserInput()
            {
                string tamperedBase64InputSignature = TamperBase64Signature(base64InputSignature);
                Assert.Throws<CryptographicException>(() => Certifier.Main(new[]{"-v", "signature", "--publickey", "public.pem", "-i", "FooBarBaz", "-s", tamperedBase64InputSignature}));
            }
            
            public static string[][] validFileSignatures = {
                new[]{"-v", "signature", "--publickey", "public.pem", "-f", "content.file", "-s", "signature.file"},
                new[]{"-v", "signature", "--publickey", "public.pem", "-i", "FooBarBaz", "-s", "signature.input"}
            };
            
            [Test, TestCaseSource("validFileSignatures")]
            public void ShouldNotThrowExceptionForValidFileSignature(string[] applicationParameters)
            {
                Assert.DoesNotThrow(() => Certifier.Main(applicationParameters));
                console.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
            }

            [Test]
            public void ShouldNotThrowExceptionWhenValidSignatureIsGivenAsParameterForFileContent()
            {
                Assert.DoesNotThrow(() => Certifier.Main(new[]{"-v", "signature", "--publickey", "public.pem", "-f", "content.file", "-s", base64FileSignature}));
                console.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
            }

            [Test]
            public void ShouldNotThrowExceptionWhenValidSignatureIsGivenAsParameterForUserInput()
            {
                Assert.DoesNotThrow(() => Certifier.Main(new[]{"-v", "signature", "--publickey", "public.pem", "-i", "FooBarBaz", "-s", base64InputSignature}));
                console.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
            }
        }
    }
}