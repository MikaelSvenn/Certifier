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
        private WriteToTextFileCommandHandler<object> commandHandler;
        private Mock<FileWrapper> file;

        [OneTimeSetUp]
        public void Setup()
        {
            file = new Mock<FileWrapper>();
            commandHandler = new WriteToTextFileCommandHandler<object>(file.Object);

            var command = new WriteToTextFileCommand<object>
            {
                FilePath = "fooDestination",
                FileContent = "barContent"
            };

            commandHandler.Execute(command);
        }

        [Test]
        public void ShouldWriteContentToGivenFileDestination()
        {
            file.Verify(f => f.WriteAllText("fooDestination", "barContent"));
        }
    }
}