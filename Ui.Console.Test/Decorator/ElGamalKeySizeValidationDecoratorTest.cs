using System;
using Core.Model;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class ElGamalKeySizeValidationDecoratorTest
    {
        private ElGamalKeySizeValidationDecorator<CreateKeyCommand<ElGamalKey>> decorator;
        private Mock<ICommandHandler<CreateKeyCommand<ElGamalKey>>> decoratedCommandHandler;

        [SetUp]
        public void Setup()
        {
            decoratedCommandHandler = new Mock<ICommandHandler<CreateKeyCommand<ElGamalKey>>>();
            decorator = new ElGamalKeySizeValidationDecorator<CreateKeyCommand<ElGamalKey>>(decoratedCommandHandler.Object);
        }

        [TestCase(2048)]
        [TestCase(3072)]
        [TestCase(4096)]
        [TestCase(6144)]
        [TestCase(8192)]
        public void ShouldNotThrowExceptionWhenKeySizeIsValid(int keySize)
        {
            var command = new CreateKeyCommand<ElGamalKey>
            {
                KeySize = keySize
            };

            Assert.DoesNotThrow(() => decorator.Execute(command));
        }

        [TestCase(1024)]
        [TestCase(1500)]
        [TestCase(2200)]
        [TestCase(3456)]
        public void ShouldThrowExceptionWhenKeySizeIsNotValid(int keySize)
        {
            var command = new CreateKeyCommand<ElGamalKey>
            {
                KeySize = keySize
            };

            Assert.Throws<ArgumentException>(() => decorator.Execute(command));
        }

        [Test]
        public void ShouldInvokeDecoratedCommand()
        {
            var command = new CreateKeyCommand<ElGamalKey>
            {
                KeySize = 2048
            };

            decorator.Execute(command);
            decoratedCommandHandler.Verify(d => d.Execute(command));
        }
    }
}