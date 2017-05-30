using System;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class FilePathValidationDecoratorTest
    {
        private FilePathValidationDecorator<ReadFileCommand<object>, object> decorator;
        private Mock<ICommandHandler<ReadFileCommand<object>>> decoratedHandler;
        private ReadFileCommand<object> command;

        [SetUp]
        public void Setup()
        {
            decoratedHandler = new Mock<ICommandHandler<ReadFileCommand<object>>>();
            decorator = new FilePathValidationDecorator<ReadFileCommand<object>, object>(decoratedHandler.Object);
        }

        [Test]
        public void ShouldExecuteDecoratedHandler()
        {
            command = Mock.Of<ReadFileCommand<object>>(c => c.FilePath == "file.path");
            decorator.Execute(command);
            decoratedHandler.Verify(h => h.Execute(command));
        }

        [Test]
        public void ShouldNotExecuteDecoratedHandlerWhenExceptionIsThrown()
        {
            command = Mock.Of<ReadFileCommand<object>>();
            Assert.Throws<ArgumentException>(() => { decorator.Execute(command); });
            decoratedHandler.Verify(h => h.Execute(command), Times.Never);
        }

        [TestFixture]
        public class ShouldThrowExceptionWhen : FilePathValidationDecoratorTest
        {
            [Test]
            public void FilePathIsNull()
            {
                command = Mock.Of<ReadFileCommand<object>>(c => c.FilePath == null);
                Assert.Throws<ArgumentException>(() => { decorator.Execute(command); });
            }

            [Test]
            public void FilePathIsEmptyString()
            {
                command = Mock.Of<ReadFileCommand<object>>(c => c.FilePath == "");
                Assert.Throws<ArgumentException>(() => { decorator.Execute(command); });
            }

            [Test]
            public void FilePathIsWhitespace()
            {
                command = Mock.Of<ReadFileCommand<object>>(c => c.FilePath == " ");
                Assert.Throws<ArgumentException>(() => { decorator.Execute(command); });
            }
        }
    }
}