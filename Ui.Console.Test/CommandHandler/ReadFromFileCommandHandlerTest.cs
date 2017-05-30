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
        private ReadFileCommandHandler<byte[]> commandHandler;
        private Mock<FileWrapper> file;
        private byte[] fileContent;

        [SetUp]
        public void Setup()
        {
            fileContent = new byte[]{ 0x07 };
            file = new Mock<FileWrapper>();
            file.Setup(f => f.ReadAllBytes("foo"))
                .Returns(fileContent);

            commandHandler = new ReadFileCommandHandler<byte[]>(file.Object);
        }

        [Test]
        public void ShouldSetReadFileAsFileContent()
        {
            var readFileCommand = new ReadFileCommand<byte[]>
            {
                FilePath = "foo"
            };

            commandHandler.Execute(readFileCommand);
            Assert.AreEqual(fileContent, readFileCommand.Result);
        }
    }
}