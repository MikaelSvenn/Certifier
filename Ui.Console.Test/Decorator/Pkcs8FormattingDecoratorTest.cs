using Core.Interfaces;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class Pkcs8FormattingDecoratorTest
    {
        private Pkcs8FormattingDecorator decorator;
        private Mock<ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>>> decoratedCommand;
        private Mock<IPkcsFormattingProvider<IAsymmetricKey>> formattingProvider;

        [SetUp]
        public void Setup()
        {
            decoratedCommand = new Mock<ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>>>();
            formattingProvider = new Mock<IPkcsFormattingProvider<IAsymmetricKey>>();

            decorator = new Pkcs8FormattingDecorator(decoratedCommand.Object, formattingProvider.Object);
        }

        [Test]
        public void ShouldInvokeDecoratedCommandWithPemFormattedContent()
        {
            var key = Mock.Of<IAsymmetricKey>();
            var command = new WriteToTextFileCommand<IAsymmetricKey>
            {
                Content = key
            };

            formattingProvider.Setup(f => f.GetAsPem(key))
                .Returns("pemFormattedFoo");

            decorator.Excecute(command);
            decoratedCommand.Verify(d => d.Excecute(It.Is<WriteToTextFileCommand<IAsymmetricKey>>(k => k.ToFile == "pemFormattedFoo")));
        }
    }
}