using System.Linq;
using System.Text;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class Pkcs8WriteFormattingDecoratorTest
    {
        private Pkcs8WriteFormattingDecorator<WriteToFileCommand<IAsymmetricKey>> decorator;
        private Mock<ICommandHandler<WriteToFileCommand<IAsymmetricKey>>> decoratedCommand;
        private Mock<IPkcsFormattingProvider<IAsymmetricKey>> formattingProvider;
        
        [SetUp]
        public void Setup()
        {
            decoratedCommand = new Mock<ICommandHandler<WriteToFileCommand<IAsymmetricKey>>>();
            formattingProvider = new Mock<IPkcsFormattingProvider<IAsymmetricKey>>();

            decorator = new Pkcs8WriteFormattingDecorator<WriteToFileCommand<IAsymmetricKey>>(decoratedCommand.Object, formattingProvider.Object);
        }

        [Test]
        public void ShouldInvokeDecoratedCommandWithPemFormattedContent()
        {
            var key = Mock.Of<IAsymmetricKey>();
            var command = new WriteToFileCommand<IAsymmetricKey>
            {
                Result = key
            };

            formattingProvider.Setup(f => f.GetAsPem(key))
                .Returns("pemFormattedFoo");

            decorator.Execute(command);
            decoratedCommand.Verify(d => d.Execute(It.Is<WriteToFileCommand<IAsymmetricKey>>(k => k.FileContent.SequenceEqual(Encoding.UTF8.GetBytes("pemFormattedFoo")))));
        }
    }
}