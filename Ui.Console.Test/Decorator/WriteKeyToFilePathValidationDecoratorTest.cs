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
        private WriteKeyToFilePathValidationDecorator<WriteFileCommand<IAsymmetricKey>> decorator;
        private Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>> decoratedHandler;
        private WriteFileCommand<IAsymmetricKey> command;

        [SetUp]
        public void Setup()
        {
            decoratedHandler = new Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>>();
            decorator = new WriteKeyToFilePathValidationDecorator<WriteFileCommand<IAsymmetricKey>>(decoratedHandler.Object);

            command = new WriteFileCommand<IAsymmetricKey>
            {
                Out = Mock.Of<IAsymmetricKey>()
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
            command.Out = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
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

            [Test]
            public void CommandHasNoOutput()
            {
                command.Out = null;
                Assert.Throws<ArgumentException>(() => { decorator.Execute(command); });
            }
        }
    }
}