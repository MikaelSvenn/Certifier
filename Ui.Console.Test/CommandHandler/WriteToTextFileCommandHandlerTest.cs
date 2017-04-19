using Core.Interfaces;
using Core.SystemWrappers;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Test.CommandHandler
{
    [TestFixture]
    public class WriteToTextFileCommandHandlerTest
    {
        private WriteToTextFileCommandHandler commandHandler;
        private Mock<FileWrapper> file;

        [OneTimeSetUp]
        public void Setup()
        {
            file = new Mock<FileWrapper>();
            commandHandler = new WriteToTextFileCommandHandler(file.Object);

            var command = new WriteToTextFileCommand<IAsymmetricKey>
            {
                Destination = "fooDestination",
                ToFile = "barContent"
            };

            commandHandler.Excecute(command);
        }

        [Test]
        public void ShouldWriteContentToGivenFileDestination()
        {
            file.Verify(f => f.WriteAllText("fooDestination", "barContent"));
        }
    }
}