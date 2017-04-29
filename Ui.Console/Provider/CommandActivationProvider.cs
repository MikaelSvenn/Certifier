using System;
using Ui.Console.Command;
using Ui.Console.Startup;

namespace Ui.Console.Provider
{
    public class CommandActivationProvider : ICommandActivationProvider
    {
        private readonly ICommandExecutor commandExecutor;
        private readonly RsaKeyCommandProvider rsaKeyCommandProvider;
        private readonly WriteToFileCommandProvider writeToFileCommandProvider;

        public CommandActivationProvider(ICommandExecutor commandExecutor, RsaKeyCommandProvider rsaKeyCommandProvider, WriteToFileCommandProvider writeToFileCommandProvider)
        {
            this.commandExecutor = commandExecutor;
            this.rsaKeyCommandProvider = rsaKeyCommandProvider;
            this.writeToFileCommandProvider = writeToFileCommandProvider;
        }

        public void CreateKey(ApplicationArguments arguments)
        {
            ICreateAsymmetricKeyCommand createKeyCommand = rsaKeyCommandProvider.GetCreateRsaKeyCommand(arguments);
            commandExecutor.Execute(createKeyCommand);

            var writePrivateKeyToFile = writeToFileCommandProvider.GetWriteKeyToTextFileCommand(createKeyCommand.Result.PrivateKey, arguments.PrivateKeyPath);
            var writePublicKeyToFile = writeToFileCommandProvider.GetWriteKeyToTextFileCommand(createKeyCommand.Result.PublicKey, arguments.PublicKeyPath);
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