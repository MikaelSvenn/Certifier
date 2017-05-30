using Core.SystemWrappers;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Test.CommandHandler
{
    [TestFixture]
    public class ReadKeyFromFileCommandHandlerTest
    {
        private Mock<FileWrapper> file;
        private ReadKeyFromFileCommandHandler commandHandler;
        private ReadKeyFromFileCommand command;
        private EncodingWrapper encoding;
        
        [OneTimeSetUp]
        public void Setup()
        {
            encoding = new EncodingWrapper();
            file = new Mock<FileWrapper>();
            file.Setup(f => f.ReadAllBytes("fromFile"))
                .Returns(encoding.GetBytes("file content"));

            commandHandler = new ReadKeyFromFileCommandHandler(file.Object);
            command = new ReadKeyFromFileCommand
            {
                FilePath = "fromFile"
            };

            commandHandler.Execute(command);
        }

        [Test]
        public void ShouldReadGivenFile()
        {
            file.Verify(f => f.ReadAllBytes("fromFile"));
        }

        [Test]
        public void ShouldSetFileContentToFromFileFieldOfGivenCommand()
        {
            Assert.AreEqual(encoding.GetBytes("file content"), command.FileContent);
        }
    }
}