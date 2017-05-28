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
            provider = new CommandActivationProvider(commandExecutor.Object, new RsaKeyCommandProvider(), new FileCommandProvider(), new SignatureCommandProvider());
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
                commandExecutor.Verify(ce => ce.ExecuteSequence(It.Is<IEnumerable<WriteToFileCommand<IAsymmetricKey>>>(w => w.First().Result == privateKey &&
                                                                                                                                w.First().FilePath == "private.pem")));
            }

            [Test]
            public void ShouldWriteCreatedPublicKeyToTextFile()
            {
                commandExecutor.Verify(ce => ce.ExecuteSequence(It.Is<IEnumerable<WriteToFileCommand<IAsymmetricKey>>>(w => w.Last().Result == publicKey &&
                                                                                                                                w.Last().FilePath == "public.pem")));
            }
        }

        [TestFixture]
        public class CreateSignature : CommandActivationProviderTest
        {
            private IAsymmetricKey key;
            private byte[] fileContent;
            private object[] sequenceExecutedCommands;
            private Signature signature;

            [OneTimeSetUp]
            public void Setup()
            {
                key = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
                fileContent = new byte[] { 0x07 };
                arguments = new ApplicationArguments
                {
                    PrivateKeyPath = "private.pem",
                    Password = "kensentme",
                    Input = "file"
                };

                commandExecutor.Setup(ce => ce.ExecuteSequence(It.IsAny<IEnumerable<object>>()))
                    .Callback<IEnumerable<object>>(sequence =>
                    {
                        var commands = sequence.ToArray();
                        var readKey = (ReadKeyFromFileCommand) commands[0];
                        readKey.Result = key;

                        var readFile = (ReadFromFileCommand) commands[1];
                        readFile.Result = fileContent;

                        sequenceExecutedCommands = commands;
                    });

                signature = new Signature();
                commandExecutor.Setup(c => c.Execute(It.IsAny<object>()))
                    .Callback<object>(c =>
                    {
                        var command = c as CreateSignatureCommand;
                        if (command != null)
                        {
                            command.Result = signature;
                        }
                    });

                provider.CreateSignature(arguments);
            }

            [Test]
            public void ShouldReadPrivateKey()
            {
                var readPrivateKeyCommand = (ReadKeyFromFileCommand)sequenceExecutedCommands[0];
                Assert.IsTrue(readPrivateKeyCommand.FilePath == "private.pem" && readPrivateKeyCommand.Password == "kensentme");
            }

            [Test]
            public void ShouldReadFileToBeSigned()
            {
                var readFileCommand = (ReadFromFileCommand)sequenceExecutedCommands[1];
                Assert.IsTrue(readFileCommand.FilePath == "file");
            }

            [Test]
            public void ShouldCreateSignatureForGivenFileWithGivenKey()
            {
                commandExecutor.Verify(ce => ce.Execute(It.Is<CreateSignatureCommand>(c => c.PrivateKey == key && c.ContentToSign == fileContent)));
            }

            [Test]
            public void ShouldWriteCreatedSignatureToTextFile()
            {
                commandExecutor.Verify(ce => ce.Execute(It.Is<WriteToFileCommand<Signature>>(c => c.Result == signature && c.FilePath == "file.signature")));
            }
        }
    }
}