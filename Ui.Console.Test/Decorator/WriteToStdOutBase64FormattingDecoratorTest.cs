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
    public class WriteToStdOutBase64FormattingDecoratorTest
    {
        private WriteToStdOutBase64FormattingDecorator<WriteToStdOutCommand<Signature>> decorator;
        private Mock<ICommandHandler<WriteToStdOutCommand<Signature>>> decoratedHandler;
        private Mock<Base64Wrapper> base64;
        private Signature signature;
        
        [SetUp]
        public void Setup()
        {
            var content = new byte[] {0x09};
            signature = new Signature
            {
                Content = content
            };
            
            base64 = new Mock<Base64Wrapper>();
            base64.Setup(b => b.ToBase64String(content))
                  .Returns("Base64FormattedContent");
            
            decoratedHandler = new Mock<ICommandHandler<WriteToStdOutCommand<Signature>>>();
            decorator = new WriteToStdOutBase64FormattingDecorator<WriteToStdOutCommand<Signature>>(decoratedHandler.Object, base64.Object);
            
            var command = new WriteToStdOutCommand<Signature>
            {
                Out = signature
            };
        }

        [Test]
        public void ShouldExecuteDecoratedHandlerWithBase64FormattedContent()
        {
            var command = new WriteToStdOutCommand<Signature>
            {
                Out = signature 
            };
            
            decorator.Execute(command);
            decoratedHandler.Verify(dh => dh.Execute(It.Is<WriteToStdOutCommand<Signature>>(c => c.ContentToStdOut == "Base64FormattedContent")));
        }
    }
}