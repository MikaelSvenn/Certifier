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
using Ui.Console;
using Ui.Console.Provider;
using Container = SimpleInjector.Container;

namespace Integration.VerifySignature.Test
{
    [TestFixture]
    public abstract class VerifySignatureTest
    {
        private Mock<FileWrapper> file;
        private Mock<ConsoleWrapper> console;
        private Dictionary<string, byte[]> files;
        private string base64FileSignature;
        private string base64InputSignature;
        private Base64Wrapper base64;
        private EncodingWrapper encoding;
        private RsaKeyProvider rsaKeyProvider;
        private DsaKeyProvider dsaKeyProvider;
        private EcKeyProvider ecKeyProvider;
        private ElGamalKeyProvider elGamalKeyProvider;
        private SignatureProvider signatureProvider;
        private Pkcs8PemFormattingProvider pkcs8PemFormatter;
        private SecureRandomGenerator random;

        [SetUp]
        public void VerifySignatureTestSetup()
        {
            file = new Mock<FileWrapper>();            
            file.Setup(f => f.ReadAllBytes(It.IsAny<string>()))
                .Returns<string>(givenFile => files[givenFile]);

            console = new Mock<ConsoleWrapper>();
            
            Container container = ContainerProvider.GetContainer();
            container.Register<FileWrapper>(() => file.Object);
            container.Register<ConsoleWrapper>(() => console.Object);

            var asymmetricKeyPairGenerator = new AsymmetricKeyPairGenerator(new SecureRandomGenerator());
            var primeMapper = new Rfc3526PrimeMapper();
            var curveNameMapper = new FieldToCurveNameMapper();
            
            rsaKeyProvider = new RsaKeyProvider(asymmetricKeyPairGenerator);
            dsaKeyProvider = new DsaKeyProvider(asymmetricKeyPairGenerator);
            ecKeyProvider = new EcKeyProvider(asymmetricKeyPairGenerator, curveNameMapper);
            elGamalKeyProvider = new ElGamalKeyProvider(asymmetricKeyPairGenerator, primeMapper);
            
            signatureProvider = new SignatureProvider(new SignatureAlgorithmIdentifierMapper(), new SecureRandomGenerator(), new SignerUtilitiesWrapper());
            pkcs8PemFormatter = new Pkcs8PemFormattingProvider(new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), rsaKeyProvider, dsaKeyProvider, ecKeyProvider, elGamalKeyProvider));

            base64 = new Base64Wrapper();
            encoding = new EncodingWrapper();
            random = new SecureRandomGenerator();
        }

        private void SetupWithRsaSignature()
        {
            byte[] fileContent = random.NextBytes(1500);
            IAsymmetricKeyPair keyPair = rsaKeyProvider.CreateKeyPair(2048);

            SetFileContent(keyPair, fileContent);
        }

        private void SetupWithDsaSignature()
        {
            byte[] fileContent = random.NextBytes(1500);
            IAsymmetricKeyPair keyPair = dsaKeyProvider.CreateKeyPair(2048);

            SetFileContent(keyPair, fileContent);
        }

        private void SetupWithEcdsaSignature()
        {
            byte[] fileContent = random.NextBytes(1500);
            IAsymmetricKeyPair keyPair = ecKeyProvider.CreateKeyPair("curve25519");

            SetFileContent(keyPair, fileContent);
        }
        
        private void SetFileContent(IAsymmetricKeyPair keyPair, byte[] fileContent)
        {
            Signature fileSignature = signatureProvider.CreateSignature(keyPair.PrivateKey, fileContent);
            base64FileSignature = base64.ToBase64String(fileSignature.Content);

            Signature inputSignature = signatureProvider.CreateSignature(keyPair.PrivateKey, encoding.GetBytes("FooBarBaz"));
            base64InputSignature = base64.ToBase64String(inputSignature.Content);

            string pemFormattedPublicKey = pkcs8PemFormatter.GetAsPem(keyPair.PublicKey);

            var tamperedFileContent = (byte[]) fileContent.Clone();
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
            Assert.Throws<CryptographicException>(() => Certifier.Main(new[] { "-v", "signature", "--publickey", "public.pem", "-f", "content.file", "-s", tamperedBase64FileSignature }));
        }

        [Test]
        public void ShouldThrowExceptionWhenInvalidSignatureIsGivenAsParameterForUserInput()
        {
            string tamperedBase64InputSignature = TamperBase64Signature(base64InputSignature);
            Assert.Throws<CryptographicException>(() => Certifier.Main(new[] { "-v", "signature", "--publickey", "public.pem", "-i", "FooBarBaz", "-s", tamperedBase64InputSignature }));
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
            Assert.DoesNotThrow(() => Certifier.Main(new[] { "-v", "signature", "--publickey", "public.pem", "-f", "content.file", "-s", base64FileSignature }));
            console.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ShouldNotThrowExceptionWhenValidSignatureIsGivenAsParameterForUserInput()
        {
            Assert.DoesNotThrow(() => Certifier.Main(new[] { "-v", "signature", "--publickey", "public.pem", "-i", "FooBarBaz", "-s", base64InputSignature }));
            console.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
        }

        private string TamperBase64Signature(string base64Signature)
        {
            char[] signatureCharacters = base64Signature.ToCharArray();
            signatureCharacters[15] = signatureCharacters[15] == 'A' ? 'a' : 'A';
            return new string(signatureCharacters);
        }
        
        [TearDown]
        public void TeardownVerifySignatureTest()
        {
            ContainerProvider.ClearContainer();
        }
        
        [TestFixture]
        public class VerifyRsaSignature : VerifySignatureTest
        {
            [SetUp]
            public void Setup()
            {
                SetupWithRsaSignature();
            }
        }

        [TestFixture]
        public class VerifyDsaSignature : VerifySignatureTest
        {
            [SetUp]
            public void Setup()
            {
                SetupWithDsaSignature();
            }
        }

        [TestFixture]
        public class VerifyEcdsaSignature : VerifySignatureTest
        {
            [SetUp]
            public void Setup()
            {
                SetupWithEcdsaSignature();
            }
        }
    }
}