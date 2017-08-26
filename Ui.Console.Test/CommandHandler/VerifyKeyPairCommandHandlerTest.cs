using System;
using System.Security.Cryptography;
using Core.Interfaces;
using Core.Model;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Test.CommandHandler
{
    [TestFixture]
    public class VerifyKeyPairCommandHandlerTest
    {
        private VerifyKeyPairCommandHandler commandHandler;
        private Mock<IKeyProvider<RsaKey>> rsaKeyProvider;
        private Mock<IKeyProvider<DsaKey>> dsaKeyProvider;
        private Mock<IEcKeyProvider> ecKeyProvider;
        
        [SetUp]
        public void Setup()
        {
            rsaKeyProvider = new Mock<IKeyProvider<RsaKey>>();
            dsaKeyProvider = new Mock<IKeyProvider<DsaKey>>();
            ecKeyProvider = new Mock<IEcKeyProvider>();
            
            commandHandler = new VerifyKeyPairCommandHandler(rsaKeyProvider.Object, dsaKeyProvider.Object, ecKeyProvider.Object);
            
            rsaKeyProvider.Setup(kp => kp.VerifyKeyPair(It.IsAny<IAsymmetricKeyPair>()))
                          .Returns(true);
            dsaKeyProvider.Setup(kp => kp.VerifyKeyPair(It.IsAny<IAsymmetricKeyPair>()))
                          .Returns(true);
            ecKeyProvider.Setup(kp => kp.VerifyKeyPair(It.IsAny<IAsymmetricKeyPair>()))
                          .Returns(true);
        }

        [TestCase(CipherType.Rsa)]
        [TestCase(CipherType.Dsa)]
        [TestCase(CipherType.Ec)]
        public void ShouldThrowExceptionWhenKeyPairIsNotValid(CipherType cipherType)
        {
            var command = new VerifyKeyPairCommand
            {
                PrivateKey = Mock.Of<IAsymmetricKey>(k => k.CipherType == cipherType),
                PublicKey = Mock.Of<IAsymmetricKey>(k => k.CipherType == cipherType)
            };
            
            rsaKeyProvider.Setup(kp => kp.VerifyKeyPair(It.IsAny<IAsymmetricKeyPair>()))
                          .Returns(false);
            dsaKeyProvider.Setup(kp => kp.VerifyKeyPair(It.IsAny<IAsymmetricKeyPair>()))
                          .Returns(false);
            ecKeyProvider.Setup(kp => kp.VerifyKeyPair(It.IsAny<IAsymmetricKeyPair>()))
                          .Returns(false);
            
            Assert.Throws<CryptographicException>(() => { commandHandler.Execute(command); });
        }

        [TestCase(CipherType.ElGamal)]
        [TestCase(CipherType.Pkcs5Encrypted)]
        [TestCase(CipherType.Pkcs12Encrypted)]
        [TestCase(CipherType.Unknown)]
        public void ShouldThrowExceptionWhenKeyTypeIsNotValid(CipherType cipherType)
        {
            var command = new VerifyKeyPairCommand
            {
                PrivateKey = Mock.Of<IAsymmetricKey>(k => k.CipherType == cipherType),
                PublicKey = Mock.Of<IAsymmetricKey>(k => k.CipherType == cipherType)
            };

            Assert.Throws<InvalidOperationException>(() => { commandHandler.Execute(command); });
        }
        
        [TestCase(CipherType.Rsa)]
        [TestCase(CipherType.Dsa)]
        [TestCase(CipherType.Ec)]
        public void ShouldNotThrowExceptionWhenKeyPairIsValid(CipherType cipherType)
        {
            var command = new VerifyKeyPairCommand
            {
                PrivateKey = Mock.Of<IAsymmetricKey>(k => k.CipherType == cipherType),
                PublicKey = Mock.Of<IAsymmetricKey>(k => k.CipherType == cipherType)
            };
            
            Assert.DoesNotThrow(() => { commandHandler.Execute(command); });
        }
    }
}