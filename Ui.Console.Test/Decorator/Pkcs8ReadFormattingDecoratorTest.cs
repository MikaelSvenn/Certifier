using Core.Interfaces;
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
    public class Pkcs8ReadFormattingDecoratorTest
    {
        private Pkcs8ReadFormattingDecorator<ReadKeyFromFileCommand> decorator;
        private Mock<ICommandHandler<ReadKeyFromFileCommand>> decoratedHandler;
        private Mock<IPkcsFormattingProvider<IAsymmetricKey>> formattingProvider;
        private EncryptedKey resultingKey;
        private ReadKeyFromFileCommand command;
        private EncodingWrapper encoding;
        
        [SetUp]
        public void Setup()
        {
            encoding = new EncodingWrapper();
            decoratedHandler = new Mock<ICommandHandler<ReadKeyFromFileCommand>>();
            formattingProvider = new Mock<IPkcsFormattingProvider<IAsymmetricKey>>();
            decorator = new Pkcs8ReadFormattingDecorator<ReadKeyFromFileCommand>(decoratedHandler.Object, formattingProvider.Object, encoding);

            resultingKey = new EncryptedKey(null, CipherType.Pkcs12Encrypted);
            
            formattingProvider.Setup(fp => fp.GetAsDer("-----BEGIN fileContent"))
                .Returns(resultingKey);

            command = new ReadKeyFromFileCommand();
            decoratedHandler.Setup(dh => dh.Execute(command))
                .Callback<ReadKeyFromFileCommand>(c => c.FileContent = encoding.GetBytes("-----BEGIN fileContent"));
        }

        [Test]
        public void ShouldSetDerFormattedContentAsCommandResult()
        {
            decorator.Execute(command);
            Assert.AreEqual(resultingKey, command.Result);
        }

        [Test]
        public void ShouldNotSetCommandResultWhenContentIsNotPemFormatted()
        {
            decoratedHandler.Setup(dh => dh.Execute(command))
                            .Callback<ReadKeyFromFileCommand>(c => c.FileContent = encoding.GetBytes("fileContent"));

            decorator.Execute(command);
            Assert.IsNull(command.Result);
        }
    }
}