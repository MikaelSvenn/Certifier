using System;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class WriteKeyToFilePathValidationDecoratorTest
    {
        private WriteKeyToFilePathValidationDecorator decorator;
        private Mock<ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>>> decoratedHandler;
        private WriteToTextFileCommand<IAsymmetricKey> command;

        [SetUp]
        public void Setup()
        {
            decoratedHandler = new Mock<ICommandHandler<WriteToTextFileCommand<IAsymmetricKey>>>();
            decorator = new WriteKeyToFilePathValidationDecorator(decoratedHandler.Object);

            command = new WriteToTextFileCommand<IAsymmetricKey>
            {
                Content = Mock.Of<IAsymmetricKey>()
            };
        }

        [Test]
        public void ShouldExecuteDecoratedHandler()
        {
            command.Destination = "foo";
            decorator.Execute(command);
            decoratedHandler.Verify(h => h.Execute(command));
        }

        [Test]
        public void ShouldNotExecuteDecoratedHandlerWhenExceptionIsThrown()
        {
            Assert.Throws<ArgumentException>(() => { decorator.Execute(command); });
            decoratedHandler.Verify(h => h.Execute(command), Times.Never);
        }

        [Test]
        public void ShouldIndicatePrivateKeyTypeInThrownException()
        {
            command.Content = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
            var exception = Assert.Throws<ArgumentException>(() => { decorator.Execute(command); });
            Assert.IsTrue(exception.Message.StartsWith("Private"));
        }

        [Test]
        public void ShouldIndicatePublicKeyTypeInThrownException()
        {
            var exception = Assert.Throws<ArgumentException>(() => { decorator.Execute(command); });
            Assert.IsTrue(exception.Message.StartsWith("Public"));
        }

        [TestFixture]
        public class ShouldThrowExceptionWhen : WriteKeyToFilePathValidationDecoratorTest
        {
            [Test]
            public void DestinationIsNull()
            {
                command.Destination = null;
                Assert.Throws<ArgumentException>(() => { decorator.Execute(command); });
            }

            [Test]
            public void DestinationIsEmptyString()
            {
                command.Destination = "";
                Assert.Throws<ArgumentException>(() => { decorator.Execute(command); });
            }

            [Test]
            public void DestinationIsWhitespace()
            {
                command.Destination = " ";
                Assert.Throws<ArgumentException>(() => { decorator.Execute(command); });
            }
        }
    }
}