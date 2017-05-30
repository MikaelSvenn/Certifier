using Core.SystemWrappers;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Test.CommandHandler
{
    [TestFixture]
    public class WriteToStdOutCommandHandler
    {
        private WriteToStdOutCommandHandler<object> commandHandler;
        private Mock<ConsoleWrapper> console;

        [SetUp]
        public void Setup()
        {
            console = new Mock<ConsoleWrapper>();
            commandHandler = new WriteToStdOutCommandHandler<object>(console.Object);
            var command = new WriteToStdOutCommand<object>
            {
                ContentToStdOut = "Written Content"
            };
            
            commandHandler.Execute(command);
        }

        [Test]
        public void ShouldWriteGivenContentToStandardOutput()
        {
            console.Verify(c => c.WriteLine("Written Content"));
        }
    }
}