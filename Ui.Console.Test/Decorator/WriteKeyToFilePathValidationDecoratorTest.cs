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
                Result = Mock.Of<IAsymmetricKey>()
            };
        }

        [Test]
        public void ShouldExecuteDecoratedHandler()
        {
            command.FilePath = "foo";
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
            command.Result = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
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
            public void FilePathIsNull()
            {
                command.FilePath = null;
                Assert.Throws<ArgumentException>(() => { decorator.Execute(command); });
            }

            [Test]
            public void FilePathIsEmptyString()
            {
                command.FilePath = "";
                Assert.Throws<ArgumentException>(() => { decorator.Execute(command); });
            }

            [Test]
            public void FilePathIsWhitespace()
            {
                command.FilePath = " ";
                Assert.Throws<ArgumentException>(() => { decorator.Execute(command); });
            }
        }
    }
}