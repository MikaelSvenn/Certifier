using System.Text;
using Core.Model;
using Core.SystemWrappers;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class WriteToFileBase64FormattingDecoratorTest
    {
        private WriteToFileBase64FormattingDecorator<WriteToTextFileCommand<Signature>> decorator;
        private Mock<ICommandHandler<WriteToTextFileCommand<Signature>>> decoratedCommandHandler;
        private Base64Wrapper base64;
        private Signature signature;
        private WriteToTextFileCommand<Signature> command;

        [OneTimeSetUp]
        public void Setup()
        {
            decoratedCommandHandler = new Mock<ICommandHandler<WriteToTextFileCommand<Signature>>>();
            base64 = new Base64Wrapper();
            decorator = new WriteToFileBase64FormattingDecorator<WriteToTextFileCommand<Signature>>(decoratedCommandHandler.Object, base64);

            signature = new Signature
            {
                Content = Encoding.Default.GetBytes("foobar")
            };

            command = new WriteToTextFileCommand<Signature>
            {
                Result = signature
            };

            decorator.Execute(command);
        }

        [Test]
        public void ShouldSetBase64FormattedSignatureAsFileContent()
        {
            var base64Result = base64.ToBase64String(signature.Content);
            Assert.AreEqual(base64Result, command.FileContent);
        }

        [Test]
        public void ShouldExecuteDecoratedCommandHandlerWithBase64FormattedSignature()
        {
            decoratedCommandHandler.Verify(d => d.Execute(command));
        }
    }
}