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
    public class VerifyRsaKeyPairCommandHandlerTest
    {
        private VerifyRsaKeyPairCommandHandler commandHandler;
        private Mock<IKeyProvider<RsaKey>> rsaKeyProvider;
        private VerifyRsaKeyPairCommand command;
        
        [SetUp]
        public void Setup()
        {
            rsaKeyProvider = new Mock<IKeyProvider<RsaKey>>();
            commandHandler = new VerifyRsaKeyPairCommandHandler(rsaKeyProvider.Object);
            
            command = new VerifyRsaKeyPairCommand
            {
                PrivateKey = Mock.Of<IAsymmetricKey>(),
                PublicKey = Mock.Of<IAsymmetricKey>()
            };
        }

        [Test]
        public void ShouldThrowExceptionWhenKeyPairIsNotValid()
        {
            rsaKeyProvider.Setup(kp => kp.VerifyKeyPair(It.IsAny<IAsymmetricKeyPair>()))
                          .Returns(false);

            Assert.Throws<CryptographicException>(() => { commandHandler.Execute(command); });
        }

        [Test]
        public void ShouldNotThrowExceptionWhenKeyPairIsValid()
        {
            rsaKeyProvider.Setup(kp => kp.VerifyKeyPair(It.IsAny<IAsymmetricKeyPair>()))
                          .Returns(true);

            Assert.DoesNotThrow(() => { commandHandler.Execute(command); });
        }
    }
}