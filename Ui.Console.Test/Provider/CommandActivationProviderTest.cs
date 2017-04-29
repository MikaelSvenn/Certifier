using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Core.Model;
using Moq;
using NUnit.Framework;
using Ui.Console.Command;
using Ui.Console.Provider;
using Ui.Console.Startup;

namespace Ui.Console.Test.Provider
{
    [TestFixture]
    public class CommandActivationProviderTest
    {
        private CommandActivationProvider provider;
        private Mock<ICommandExecutor> commandExecutor;
        private ApplicationArguments arguments;

        [OneTimeSetUp]
        public void SetupCommandActivationProviderTest()
        {
            commandExecutor = new Mock<ICommandExecutor>();
            provider = new CommandActivationProvider(commandExecutor.Object, new RsaKeyCommandProvider(), new WriteToFileCommandProvider());
        }

        [TestFixture]
        public class CreateKey : CommandActivationProviderTest
        {
            private IAsymmetricKey privateKey;
            private IAsymmetricKey publicKey;

            [OneTimeSetUp]
            public void Setup()
            {
                privateKey = Mock.Of<IAsymmetricKey>();
                publicKey = Mock.Of<IAsymmetricKey>();

                var createdKeyPair = new AsymmetricKeyPair(privateKey, publicKey);
                commandExecutor.Setup(c => c.Execute(It.IsAny<object>()))
                    .Callback<object>(c => ((ICommandWithResult<IAsymmetricKeyPair>)c).Result = createdKeyPair);

                arguments = new ApplicationArguments
                {
                    KeySize = 1024,
                    EncryptionType = KeyEncryptionType.None,
                    PrivateKeyPath = "private.pem",
                    PublicKeyPath = "public.pem"
                };

                provider.CreateKey(arguments);
            }

            [Test]
            public void ShouldCreateRsaKeyPair()
            {
                commandExecutor.Verify(ce => ce.Execute(It.Is<CreateRsaKeyCommand>(c => c.EncryptionType == KeyEncryptionType.None && c.KeySize == 1024)));
            }

            [Test]
            public void ShouldWriteCreatedPrivateKeyToTextFile()
            {
                commandExecutor.Verify(ce => ce.ExecuteSequence(It.Is<IEnumerable<WriteToTextFileCommand<IAsymmetricKey>>>(w => w.First().Content == privateKey &&
                                                                                                                                w.First().Destination == "private.pem")));
            }

            [Test]
            public void ShouldWriteCreatedPublicKeyToTextFile()
            {
                commandExecutor.Verify(ce => ce.ExecuteSequence(It.Is<IEnumerable<WriteToTextFileCommand<IAsymmetricKey>>>(w => w.Last().Content == publicKey &&
                                                                                                                                w.Last().Destination == "public.pem")));
            }
        }
    }
}