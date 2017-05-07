using System;
using Ui.Console.Command;
using Ui.Console.Startup;

namespace Ui.Console.Provider
{
    public class CommandActivationProvider : ICommandActivationProvider
    {
        private readonly ICommandExecutor commandExecutor;
        private readonly RsaKeyCommandProvider rsaKeyCommandProvider;
        private readonly FileCommandProvider fileCommandProvider;

        public CommandActivationProvider(ICommandExecutor commandExecutor, RsaKeyCommandProvider rsaKeyCommandProvider, FileCommandProvider fileCommandProvider)
        {
            this.commandExecutor = commandExecutor;
            this.rsaKeyCommandProvider = rsaKeyCommandProvider;
            this.fileCommandProvider = fileCommandProvider;
        }

        public void CreateKey(ApplicationArguments arguments)
        {
            ICreateAsymmetricKeyCommand createKeyCommand = rsaKeyCommandProvider.GetCreateRsaKeyCommand(arguments);
            commandExecutor.Execute(createKeyCommand);

            var writePrivateKeyToFile = fileCommandProvider.GetWriteKeyToTextFileCommand(createKeyCommand.Result.PrivateKey, arguments.PrivateKeyPath);
            var writePublicKeyToFile = fileCommandProvider.GetWriteKeyToTextFileCommand(createKeyCommand.Result.PublicKey, arguments.PublicKeyPath);
            commandExecutor.ExecuteSequence(new []{writePrivateKeyToFile, writePublicKeyToFile});
        }

        public void CreateSignature(ApplicationArguments arguments)
        {
            throw new NotImplementedException();
        }

        public void VerifySignature(ApplicationArguments arguments)
        {
            throw new NotImplementedException();
        }

        public void VerifyKeyPair(ApplicationArguments arguments)
        {
            throw new NotImplementedException();
        }
    }
}