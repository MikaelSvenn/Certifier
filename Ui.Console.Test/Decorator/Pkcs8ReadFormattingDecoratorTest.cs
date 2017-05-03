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
        private Pkcs8ReadFormattingDecorator decorator;
        private Mock<ICommandHandler<ReadFromTextFileCommand<IAsymmetricKey>>> decoratedHandler;
        private Mock<IPkcsFormattingProvider<IAsymmetricKey>> formattingProvider;
        private EncryptedKey resultingKey;
        private ReadFromTextFileCommand<IAsymmetricKey> command;

        [SetUp]
        public void Setup()
        {
            decoratedHandler = new Mock<ICommandHandler<ReadFromTextFileCommand<IAsymmetricKey>>>();
            formattingProvider = new Mock<IPkcsFormattingProvider<IAsymmetricKey>>();
            decorator = new Pkcs8ReadFormattingDecorator(decoratedHandler.Object, formattingProvider.Object);

            resultingKey = new EncryptedKey(null, CipherType.Pkcs12Encrypted);
            formattingProvider.Setup(fp => fp.GetAsDer("fileContent"))
                .Returns(resultingKey);

            command = new ReadFromTextFileCommand<IAsymmetricKey>();
            decoratedHandler.Setup(dh => dh.Execute(command))
                .Callback<ReadFromTextFileCommand<IAsymmetricKey>>(c => c.ContentFromFile = "fileContent");

            decorator.Execute(command);
        }

        [Test]
        public void ShouldSetDerFormattedContentAsCommandResult()
        {
            Assert.AreEqual(resultingKey, command.Result);
        }
    }
}