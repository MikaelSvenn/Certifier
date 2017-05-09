using Core.SystemWrappers;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Test.CommandHandler
{
    [TestFixture]
    public class ReadFromFileCommandHandlerTest
    {
        private ReadFromFileCommandHandler commandHandler;
        private Mock<FileWrapper> file;
        private byte[] fileContent;

        [SetUp]
        public void Setup()
        {
            fileContent = new byte[]{ 0x07 };
            file = new Mock<FileWrapper>();
            file.Setup(f => f.ReadAllBytes("foo"))
                .Returns(fileContent);

            commandHandler = new ReadFromFileCommandHandler(file.Object);
        }

        [Test]
        public void ShouldSetReadFileAsCommandResult()
        {
            var readFileCommand = new ReadFromFileCommand
            {
                FilePath = "foo"
            };

            commandHandler.Execute(readFileCommand);
            Assert.AreEqual(fileContent, readFileCommand.Result);
        }
    }
}