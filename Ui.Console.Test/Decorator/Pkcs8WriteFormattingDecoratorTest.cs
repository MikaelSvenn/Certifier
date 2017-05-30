using System.Linq;
using Core.Interfaces;
using Core.SystemWrappers;
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
        private Pkcs8WriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>> decorator;
        private Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>> decoratedCommand;
        private Mock<IPkcsFormattingProvider<IAsymmetricKey>> formattingProvider;
        private EncodingWrapper encoding;
        
        [SetUp]
        public void Setup()
        {
            decoratedCommand = new Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>>();
            formattingProvider = new Mock<IPkcsFormattingProvider<IAsymmetricKey>>();

            encoding = new EncodingWrapper();
            decorator = new Pkcs8WriteFormattingDecorator<WriteFileCommand<IAsymmetricKey>>(decoratedCommand.Object, formattingProvider.Object, encoding);
        }

        [Test]
        public void ShouldInvokeDecoratedCommandWithPemFormattedContent()
        {
            var key = Mock.Of<IAsymmetricKey>();
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = key
            };

            formattingProvider.Setup(f => f.GetAsPem(key))
                .Returns("pemFormattedFoo");

            decorator.Execute(command);
            decoratedCommand.Verify(d => d.Execute(It.Is<WriteFileCommand<IAsymmetricKey>>(k => k.FileContent.SequenceEqual(encoding.GetBytes("pemFormattedFoo")))));
        }
    }
}