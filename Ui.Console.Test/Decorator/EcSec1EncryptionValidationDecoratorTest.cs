using System;
using Core.Interfaces;
using Core.Model;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class EcSec1EncryptionValidationDecoratorTest
    {
        private EcSec1EncryptionValidationDecorator<WriteFileCommand<IAsymmetricKey>> decorator;
        private Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>> decoratedCommandHandler;
        
        [SetUp]
        public void Setup()
        {
            decoratedCommandHandler = new Mock<ICommandHandler<WriteFileCommand<IAsymmetricKey>>>();
            decorator = new EcSec1EncryptionValidationDecorator<WriteFileCommand<IAsymmetricKey>>(decoratedCommandHandler.Object);
        }

        [Test]
        public void ShouldThrowWhenEncryptionIsAppliedToSec1Key()
        {
            
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                ContentType = ContentType.Sec1,
                EncryptionType = EncryptionType.Pkcs
            };

            Assert.Throws<InvalidOperationException>(() => decorator.Execute(command));
        }

        [Test]
        public void ShouldExecuteDecoratedCommandHandler()
        {
            var command = new WriteFileCommand<IAsymmetricKey>
            {
                ContentType = ContentType.Sec1
            };
            
            decorator.Execute(command);
            decoratedCommandHandler.Verify(ch => ch.Execute(command));
        }
    }
}