using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Test.CommandHandler
{
    [TestFixture]
    public class CreateElGamalKeyCommandHandlerTest
    {
        private ICommandHandler<CreateKeyCommand<ElGamalKey>> commandHandler;
        private Mock<IElGamalKeyProvider> keyProvider;
        private IAsymmetricKeyPair keyPair;
        private CreateKeyCommand<ElGamalKey> command;
        private Mock<ConsoleWrapper> console;
        
        [SetUp]
        public void Setup()
        {
            keyPair = Mock.Of<IAsymmetricKeyPair>();

            keyProvider = new Mock<IElGamalKeyProvider>();
            keyProvider.Setup(kp => kp.CreateKeyPair(2048, true))
                       .Returns(keyPair);

            console = new Mock<ConsoleWrapper>();
            commandHandler = new CreateElGamalKeyCommandHandler(keyProvider.Object, console.Object);
            command = new CreateKeyCommand<ElGamalKey>
            {
                KeySize = 2048,
                UseRfc3526Prime = true
            };

            commandHandler.Execute(command);
        }

        [Test]
        public void ShouldIndicatePerformanceWarningWhenRfc3526PrimesAreNotUsed()
        {
            command.UseRfc3526Prime = false;
            commandHandler.Execute(command);
            
            console.Verify(c => c.WriteLine("NOTE: Depending on system performance, creating an ElGamal keypair may take from 20 minutes up to several hours."));
        }

        [Test]
        public void ShouldNotIndicatePerformanceWarningWhenRfc3526PrimesAreUsed()
        {
            console.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
        }
        
        [Test]
        public void ShouldSetCreatedKeyPairAsCommandResult()
        {
            Assert.AreEqual(keyPair, command.Result);
        }
    }
}