using Core.Interfaces;
using Ui.Console.Command;
using Ui.Console.Startup;

namespace Ui.Console.Provider
{
    public class KeyCommandActivationProvider : IKeyCommandActivationProvider
    {
        private readonly ICommandExecutor commandExecutor;
        private readonly KeyCommandProvider keyCommandProvider;
        private readonly FileCommandProvider fileCommandProvider;

        public KeyCommandActivationProvider(ICommandExecutor commandExecutor, KeyCommandProvider keyCommandProvider, FileCommandProvider fileCommandProvider)
        {
            this.commandExecutor = commandExecutor;
            this.keyCommandProvider = keyCommandProvider;
            this.fileCommandProvider = fileCommandProvider;
        }

        public void CreateKeyPair(ApplicationArguments arguments)
        {
            ICreateAsymmetricKeyCommand createKeyCommand = keyCommandProvider.GetCreateKeyCommand(arguments.KeySize, arguments.EncryptionType, arguments.Password);
            commandExecutor.Execute(createKeyCommand);

            WriteFileCommand<IAsymmetricKey> writePrivateKeyToFile = fileCommandProvider.GetWriteToFileCommand<IAsymmetricKey>(createKeyCommand.Result.PrivateKey, arguments.PrivateKeyPath);
            WriteFileCommand<IAsymmetricKey> writePublicKeyToFile = fileCommandProvider.GetWriteToFileCommand<IAsymmetricKey>(createKeyCommand.Result.PublicKey, arguments.PublicKeyPath);
            commandExecutor.ExecuteSequence(new []{writePrivateKeyToFile, writePublicKeyToFile});
        }
        
        public void VerifyKeyPair(ApplicationArguments arguments)
        {
            ReadKeyFromFileCommand readPublicKeyFromFile = fileCommandProvider.GetReadPublicKeyFromFileCommand(arguments.PublicKeyPath);
            ReadKeyFromFileCommand readPrivateKeyFromFile = fileCommandProvider.GetReadPrivateKeyFromFileCommand(arguments.PrivateKeyPath, arguments.Password);
            commandExecutor.ExecuteSequence(new []{readPrivateKeyFromFile, readPublicKeyFromFile});

            IVerifyKeyPairCommand verifyKeyPairCommand = keyCommandProvider.GetVerifyKeyPairCommand(readPublicKeyFromFile.Result, readPrivateKeyFromFile.Result);
            commandExecutor.Execute(verifyKeyPairCommand);
        }
    }
}