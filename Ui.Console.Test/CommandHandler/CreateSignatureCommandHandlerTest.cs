using Core.Interfaces;
using Core.Model;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Test.CommandHandler
{
    [TestFixture]
    public class CreateSignatureCommandHandlerTest
    {
        private CreateSignatureCommandHandler commandHandler;
        private Mock<ISignatureProvider> signatureProvider;
        private IAsymmetricKey privateKey;
        private Signature signature;

        [SetUp]
        public void Setup()
        {
            privateKey = Mock.Of<IAsymmetricKey>();
            signature = new Signature();

            signatureProvider = new Mock<ISignatureProvider>();
            signatureProvider.Setup(s => s.CreateSignature(privateKey, It.IsAny<byte[]>()))
                .Returns(signature);

            commandHandler = new CreateSignatureCommandHandler(signatureProvider.Object);
        }

        [Test]
        public void ShouldSetCreatedSignatureAsCommandResult()
        {
            var command = new CreateSignatureCommand
            {
                ContentToSign = new byte[]{0x07},
                PrivateKey = privateKey
            };

            commandHandler.Execute(command);
            Assert.AreEqual(signature, command.Result);
        }
    }
}