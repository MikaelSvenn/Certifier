using Core.Interfaces;
using Core.Model;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Test.CommandHandler
{
    [TestFixture]
    public class CreateRsaKeyCommandHandlerTest
    {
        private ICommandHandler<CreateRsaKeyCommand> commandHandler;
        private Mock<IAsymmetricKeyProvider<RsaKey>> keyProvider;
        private IAsymmetricKeyPair keyPair;

        [SetUp]
        public void Setup()
        {
            keyPair = Mock.Of<IAsymmetricKeyPair>();

            keyProvider = new Mock<IAsymmetricKeyProvider<RsaKey>>();
            keyProvider.Setup(kp => kp.CreateKeyPair(4096))
                .Returns(keyPair);

            commandHandler = new CreateRsaKeyCommandHandler(keyProvider.Object);
        }

        [Test]
        public void ShouldSetCreatedKeyPairAsCommandResult()
        {
            var command = new CreateRsaKeyCommand
            {
                KeySize = 4096
            };

            commandHandler.Excecute(command);
            Assert.AreEqual(keyPair, command.Result);
        }
    }
}