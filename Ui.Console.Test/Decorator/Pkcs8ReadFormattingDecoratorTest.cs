using System.Text;
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
    public class Pkcs8ReadFormattingDecoratorTest
    {
        private Pkcs8ReadFormattingDecorator<ReadKeyFromFileCommand> decorator;
        private Mock<ICommandHandler<ReadKeyFromFileCommand>> decoratedHandler;
        private Mock<IPkcsFormattingProvider<IAsymmetricKey>> formattingProvider;
        private EncryptedKey resultingKey;
        private ReadKeyFromFileCommand command;

        [SetUp]
        public void Setup()
        {
            decoratedHandler = new Mock<ICommandHandler<ReadKeyFromFileCommand>>();
            formattingProvider = new Mock<IPkcsFormattingProvider<IAsymmetricKey>>();
            decorator = new Pkcs8ReadFormattingDecorator<ReadKeyFromFileCommand>(decoratedHandler.Object, formattingProvider.Object);

            resultingKey = new EncryptedKey(null, CipherType.Pkcs12Encrypted);
            
            formattingProvider.Setup(fp => fp.GetAsDer("fileContent"))
                .Returns(resultingKey);

            command = new ReadKeyFromFileCommand();
            decoratedHandler.Setup(dh => dh.Execute(command))
                .Callback<ReadKeyFromFileCommand>(c => c.FileContent = Encoding.UTF8.GetBytes("fileContent"));

            decorator.Execute(command);
        }

        [Test]
        public void ShouldSetDerFormattedContentAsCommandResult()
        {
            Assert.AreEqual(resultingKey, command.Result);
        }
    }
}