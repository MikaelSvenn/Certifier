using System.Collections.Generic;
using System.Security.Cryptography;
using Core.Interfaces;
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
    public class VerifyRsaKeyPairTest
    {
        private Dictionary<string, byte[]> files;
        private Mock<FileWrapper> file;
        private Mock<ConsoleWrapper> console;
        
        [SetUp]
        public void Setup()
        {
            var keyProvider = new RsaKeyProvider(new AsymmetricKeyPairGenerator(new SecureRandomGenerator()));
            var pkcs8Formatter = new Pkcs8FormattingProvider(new AsymmetricKeyProvider(new OidToCipherTypeMapper(), new KeyInfoWrapper(), keyProvider, null));
            
            file = new Mock<FileWrapper>();
            file = new Mock<FileWrapper>();            
            file.Setup(f => f.ReadAllBytes(It.IsAny<string>()))
                .Returns<string>(givenFile => files[givenFile]);
            
            var encoding = new EncodingWrapper();
            files = new Dictionary<string, byte[]>();
            
            IAsymmetricKeyPair keyPair = keyProvider.CreateKeyPair(2048);
            string firstPublicKey = pkcs8Formatter.GetAsPem(keyPair.PublicKey);
            string firstPrivateKey = pkcs8Formatter.GetAsPem(keyPair.PrivateKey);
            
            files.Add("private.first", encoding.GetBytes(firstPrivateKey));
            files.Add("public.first", encoding.GetBytes(firstPublicKey));
            
            keyPair = keyProvider.CreateKeyPair(2048);
            
            string secondPublicKey = pkcs8Formatter.GetAsPem(keyPair.PublicKey);
            string secondPrivateKey = pkcs8Formatter.GetAsPem(keyPair.PrivateKey);
            files.Add("private.second", encoding.GetBytes(secondPrivateKey));
            files.Add("public.second", encoding.GetBytes(secondPublicKey));
            
            console = new Mock<ConsoleWrapper>();
            
            Container container = ContainerProvider.GetContainer();
            container.Register<FileWrapper>(() => file.Object);
            container.Register<ConsoleWrapper>(() => console.Object);
        }
        
        [TearDown]
        public void TeardownVerifyKeyPairTest()
        {
            ContainerProvider.ClearContainer();
        }
        
        [Test]
        public void ShouldNotThrowExceptionWhenKeyPairIsValid()
        {
            Assert.DoesNotThrow(() => Certifier.Main(new[]{"-v", "key", "--publickey", "public.first", "--privatekey", "private.first"}));
            console.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ShouldThrowExceptionWhenKeyPairIsNotValid()
        {
            Assert.Throws<CryptographicException>(() => Certifier.Main(new[]{"-v", "key", "--publickey", "public.first", "--privatekey", "private.second"}));
        }
    }
}