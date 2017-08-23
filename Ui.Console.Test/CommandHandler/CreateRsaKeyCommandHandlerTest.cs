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
        private ICommandHandler<CreateKeyCommand<RsaKey>> commandHandler;
        private Mock<IKeyProvider<RsaKey>> keyProvider;
        private IAsymmetricKeyPair keyPair;

        [SetUp]
        public void Setup()
        {
            keyPair = Mock.Of<IAsymmetricKeyPair>();

            keyProvider = new Mock<IKeyProvider<RsaKey>>();
            keyProvider.Setup(kp => kp.CreateKeyPair(4096))
                .Returns(keyPair);

            commandHandler = new CreateRsaKeyCommandHandler(keyProvider.Object);
        }

        [Test]
        public void ShouldSetCreatedKeyPairAsCommandResult()
        {
            var command = new CreateKeyCommand<RsaKey>
            {
                KeySize = 4096
            };

            commandHandler.Execute(command);
            Assert.AreEqual(keyPair, command.Result);
        }
    }
}