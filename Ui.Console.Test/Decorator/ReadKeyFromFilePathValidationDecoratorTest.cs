using System;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class ReadKeyFromFilePathValidationDecoratorTest
    {
        private ReadKeyFromFilePathValidationDecorator<ReadKeyFromFileCommand> decorator;
        private Mock<ICommandHandler<ReadKeyFromFileCommand>> decoratedHandler;

        [SetUp]
        public void Setup()
        {
            decoratedHandler = new Mock<ICommandHandler<ReadKeyFromFileCommand>>();
            decorator = new ReadKeyFromFilePathValidationDecorator<ReadKeyFromFileCommand>(decoratedHandler.Object);
        }

        [Test]
        public void ShouldExecuteDecoratedHandler()
        {
            var command = new ReadKeyFromFileCommand
            {
                FilePath = "foo"
            };
            
            decorator.Execute(command);
            decoratedHandler.Verify(h => h.Execute(command));
        }

        [Test]
        public void ShouldIndicatePrivateKeyInExceptionWhenCommandIsForPrivateKey()
        {
            var command = new ReadKeyFromFileCommand
            {
                IsPrivateKey = true
            };
            
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                decorator.Execute(command);
            });
            
            Assert.AreEqual("Private key file or path is required.", exception.Message);
        }

        [Test]
        public void ShouldIndicatePublicKeyInExceptionWhenCommandIsNotForPrivateKey()
        {
            var command = new ReadKeyFromFileCommand();
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                decorator.Execute(command);
            });
            
            Assert.AreEqual("Public key file or path is required.", exception.Message);
        }
        
        [TestFixture]
        public class ShouldThrowExceptionWhen : ReadKeyFromFilePathValidationDecoratorTest
        {
            [Test]
            public void FilePathIsNull()
            {
                var command = new ReadKeyFromFileCommand();
                Assert.Throws<ArgumentException>(() =>
                {
                    decorator.Execute(command);
                });
            }

            [Test]
            public void FilePathIsEmpty()
            {
                var command = new ReadKeyFromFileCommand
                {
                    FilePath = ""
                };
                
                Assert.Throws<ArgumentException>(() =>
                {
                    decorator.Execute(command);
                });
            }

            [Test]
            public void FilePathIsWhiteSpace()
            {
                var command = new ReadKeyFromFileCommand
                {
                    FilePath = "   "
                };
                
                Assert.Throws<ArgumentException>(() =>
                {
                    decorator.Execute(command);
                });
            }
        }
    }
}