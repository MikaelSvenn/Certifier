using System;
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
            public void ShouldWriteCreatedPrivateKeyToFile()
            {
                commandExecutor.Verify(ce => ce.ExecuteSequence(It.Is<IEnumerable<WriteFileCommand<IAsymmetricKey>>>(w => w.First().Out == privateKey &&
                                                                                                                                w.First().FilePath == "private.pem")));
            }

            [Test]
            public void ShouldWriteCreatedPublicKeyToFile()
            {
                commandExecutor.Verify(ce => ce.ExecuteSequence(It.Is<IEnumerable<WriteFileCommand<IAsymmetricKey>>>(w => w.Last().Out == publicKey &&
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

            [SetUp]
            public void Setup()
            {
                key = Mock.Of<IAsymmetricKey>(k => k.IsPrivateKey);
                fileContent = new byte[] { 0x07 };
                arguments = new ApplicationArguments
                {
                    PrivateKeyPath = "private.pem",
                    Password = "kensentme",
                    Input = "file",
                    Output = "file.signature"
                };

                commandExecutor.Setup(ce => ce.ExecuteSequence(It.IsAny<IEnumerable<object>>()))
                    .Callback<IEnumerable<object>>(sequence =>
                    {
                        var commands = sequence.ToArray();
                        var readKey = (ReadKeyFromFileCommand) commands[0];
                        readKey.Result = key;

                        var readFile = (ReadFileCommand<byte[]>) commands[1];
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
            }

            [Test]
            public void ShouldReadPrivateKey()
            {
                provider.CreateSignature(arguments);
                
                var readPrivateKeyCommand = (ReadKeyFromFileCommand)sequenceExecutedCommands[0];
                Assert.IsTrue(readPrivateKeyCommand.FilePath == "private.pem" && readPrivateKeyCommand.Password == "kensentme");
            }

            [Test]
            public void ShouldReadFileToBeSigned()
            {
                provider.CreateSignature(arguments);
                
                var readFileCommand = (ReadFileCommand<byte[]>)sequenceExecutedCommands[1];
                Assert.IsTrue(readFileCommand.FilePath == "file");
            }

            [Test]
            public void ShouldCreateSignatureForGivenFileWithGivenKey()
            {
                provider.CreateSignature(arguments);
                commandExecutor.Verify(ce => ce.Execute(It.Is<CreateSignatureCommand>(c => c.PrivateKey == key && c.ContentToSign == fileContent)));
            }

            [Test]
            public void ShouldWriteCreatedSignatureToTextFileWhenOutputIsSpecified()
            {
                provider.CreateSignature(arguments);
                commandExecutor.Verify(ce => ce.Execute(It.Is<WriteFileCommand<Signature>>(c => c.Out == signature && c.FilePath == "file.signature")));
            }

            [Test]
            public void ShouldNotWriteCreatedSignatureToTextFileWhenoutputIsNotSpecified()
            {
                arguments.Output = string.Empty;
                provider.CreateSignature(arguments);
                commandExecutor.Verify(ce => ce.Execute(It.Is<WriteFileCommand<Signature>>(c => c.Out == signature && c.FilePath == "file.signature")), Times.Never());
            }
            
            [Test]
            public void ShouldWriteCreatedSignatureToStdOutWhenOutputIsNotSpecified()
            {
                arguments.Output = string.Empty;
                provider.CreateSignature(arguments);
                commandExecutor.Verify(ce => ce.Execute(It.Is<WriteToStdOutCommand<Signature>>(c => c.Out == signature)));
            }

            [Test]
            public void ShouldNotWriteCreatedSignatureToStdOutwhenOutputIsSpecified()
            {
                provider.CreateSignature(arguments);
                commandExecutor.Verify(ce => ce.Execute(It.Is<WriteToStdOutCommand<Signature>>(c => c.Out == signature)), Times.Never());
            }
        }
    }
}