using Core.Interfaces;
using Core.SystemWrappers;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Test.CommandHandler
{
    [TestFixture]
    public class ReadKeyFromTextFileCommandHandlerTest
    {
        private Mock<FileWrapper> file;
        private ReadKeyFromTextFileCommandHandler commandHandler;
        private ReadFromTextFileCommand<IAsymmetricKey> command;

        [OneTimeSetUp]
        public void Setup()
        {
            file = new Mock<FileWrapper>();
            file.Setup(f => f.ReadAllText("fromFile"))
                .Returns("file content");

            commandHandler = new ReadKeyFromTextFileCommandHandler(file.Object);
            command = new ReadFromTextFileCommand<IAsymmetricKey>
            {
                FilePath = "fromFile"
            };

            commandHandler.Execute(command);
        }

        [Test]
        public void ShouldReadGivenFile()
        {
            file.Verify(f => f.ReadAllText("fromFile"));
        }

        [Test]
        public void ShouldSetFileContentToFromFileFieldOfGivenCommand()
        {
            Assert.AreEqual("file content", command.ContentFromFile);
        }
    }
}