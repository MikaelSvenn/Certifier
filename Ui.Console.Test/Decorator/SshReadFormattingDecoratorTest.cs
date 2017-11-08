using Core.Interfaces;
using Core.SystemWrappers;
using Crypto.Providers;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Decorator;

namespace Ui.Console.Test.Decorator
{
    [TestFixture]
    public class SshReadFormattingDecoratorTest
    {
        private SshReadFormattingDecorator<ReadKeyFromFileCommand> decorator;
        private Mock<ICommandHandler<ReadKeyFromFileCommand>> decoratedHandler;
        private Mock<ISshFormattingProvider> formattingProvider;
        private ReadKeyFromFileCommand command;
        private EncodingWrapper encoding;
        private IAsymmetricKey key;
        
        [SetUp]
        public void Setup()
        {
            encoding = new EncodingWrapper();
            decoratedHandler = new Mock<ICommandHandler<ReadKeyFromFileCommand>>();
            formattingProvider = new Mock<ISshFormattingProvider>();
            decorator = new SshReadFormattingDecorator<ReadKeyFromFileCommand>(decoratedHandler.Object, formattingProvider.Object, encoding);

            key = Mock.Of<IAsymmetricKey>();
            command = new ReadKeyFromFileCommand();
            
            decoratedHandler.Setup(dh => dh.Execute(command))
                .Callback<ReadKeyFromFileCommand>(c => c.FileContent = encoding.GetBytes("foobar"));

            formattingProvider.Setup(fp => fp.IsSshKey("foobar"))
                              .Returns(true);
            formattingProvider.Setup(fp => fp.GetAsDer("foobar"))
                              .Returns(key);
        }
        
        [Test]
        public void ShouldSetSshFormattedContentAsCommandResult()
        {
            decorator.Execute(command);
            Assert.AreEqual(key, command.Result);
        }
        
        [Test]
        public void ShouldNotSetCommandResultWhenResultIsAlreadySet()
        {
            var preSetKey = Mock.Of<IAsymmetricKey>();
            command.Result = preSetKey;
            
            decorator.Execute(command);
            Assert.AreEqual(preSetKey, command.Result);
        }

        [Test]
        public void ShouldNotSetCommandResultWhenFileContentIsNotSshKey()
        {
            formattingProvider.Setup(fp => fp.IsSshKey("foobar"))
                              .Returns(false);
            
            Assert.IsNull(command.Result);
        }
    }
}