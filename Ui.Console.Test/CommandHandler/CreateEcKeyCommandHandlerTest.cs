using Core.Interfaces;
using Core.Model;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Test.CommandHandler
{
    [TestFixture]
    public class CreateEcKeyCommandHandlerTest
    {
        private CreateEcKeyCommandHandler commandHandler;
        private Mock<IEcKeyProvider> keyProvider;
        private IAsymmetricKeyPair keyPair;
        private CreateKeyCommand<EcKey> command;
        
        [SetUp]
        public void Setup()
        {
            keyPair = new AsymmetricKeyPair(null, null);    
            keyProvider = new Mock<IEcKeyProvider>();
            keyProvider.Setup(k => k.CreateKeyPair("foobar"))
                       .Returns(keyPair);

            command = new CreateKeyCommand<EcKey>()
            {
                Curve = "foobar"
            };
            
            commandHandler = new CreateEcKeyCommandHandler(keyProvider.Object);
            commandHandler.Execute(command);
        }

        [Test]
        public void ShouldCreateKey()
        {
            keyProvider.Verify(kp => kp.CreateKeyPair("foobar"));
        }

        [Test]
        public void ShouldSetCreatedKeyAsCommandResult()
        {
            Assert.AreEqual(keyPair, command.Result);
        }
    }
}