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
    public class DsaKeySizeValidationDecoratorTest
    {
        private DsaKeySizeValidationDecorator<CreateKeyCommand<DsaKey>> decorator;
        private Mock<ICommandHandler<CreateKeyCommand<DsaKey>>> decoratedCommandHandler;

        [SetUp]
        public void Setup()
        {
            decoratedCommandHandler = new Mock<ICommandHandler<CreateKeyCommand<DsaKey>>>();
            decorator = new DsaKeySizeValidationDecorator<CreateKeyCommand<DsaKey>>(decoratedCommandHandler.Object);
        }

        [TestCase(2048)]
        [TestCase(3072)]
        public void ShouldNotThrowExceptionWhenKeySizeIsValid(int keySize)
        {
            var command = new CreateKeyCommand<DsaKey>
            {
                KeySize = keySize
            };
            
            Assert.DoesNotThrow(() => {decorator.Execute(command);});
        }

        [TestCase(1024)]
        [TestCase(4096)]
        [TestCase(10000)]
        public void ShouldThrowExceptionWhenKeySizeIsNotValid(int keySize)
        {
            var command = new CreateKeyCommand<DsaKey>
            {
                KeySize = keySize
            };
            
            Assert.Throws<ArgumentException>(() => {decorator.Execute(command);});
        }

        [Test]
        public void ShouldInvokeDecoratedCommandHandler()
        {
            var command = new CreateKeyCommand<DsaKey>
            {
                KeySize = 2048
            };
            
            decorator.Execute(command);
            decoratedCommandHandler.Verify(dch => dch.Execute(command));
        }
    }
}