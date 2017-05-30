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
        private WriteToFileCommandHandler<object> commandHandler;
        private Mock<FileWrapper> file;
        private byte[] fileContent;
        
        [OneTimeSetUp]
        public void Setup()
        {
            file = new Mock<FileWrapper>();
            commandHandler = new WriteToFileCommandHandler<object>(file.Object);

            var encoding = new EncodingWrapper();
            fileContent = encoding.GetBytes("barContent");
            
            var command = new WriteFileCommand<object>
            {
                FilePath = "fooDestination",
                FileContent = fileContent
            };

            commandHandler.Execute(command);
        }

        [Test]
        public void ShouldWriteContentToGivenFileDestination()
        {
            file.Verify(f => f.WriteAllBytes("fooDestination", fileContent));
        }
    }
}