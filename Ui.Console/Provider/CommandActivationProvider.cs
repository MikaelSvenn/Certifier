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
        private readonly SignatureCommandProvider signatureCommandProvider;

        public CommandActivationProvider(ICommandExecutor commandExecutor, RsaKeyCommandProvider rsaKeyCommandProvider, FileCommandProvider fileCommandProvider, SignatureCommandProvider signatureCommandProvider)
        {
            this.commandExecutor = commandExecutor;
            this.rsaKeyCommandProvider = rsaKeyCommandProvider;
            this.fileCommandProvider = fileCommandProvider;
            this.signatureCommandProvider = signatureCommandProvider;
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
            var readPrivateKeyFromFile = fileCommandProvider.GetReadKeyFromTextFileCommand(arguments.PrivateKeyPath, arguments.Password);
            var readFileToSign = fileCommandProvider.GetReadFormFileCommand(arguments.DataPath);
            commandExecutor.ExecuteSequence(new dynamic[]{readPrivateKeyFromFile, readFileToSign});

            var createSignature = signatureCommandProvider.GetCreateSignatureCommand(readPrivateKeyFromFile.Result, readFileToSign.Result);
            commandExecutor.Execute(createSignature);

            var writeSignatureTofile = fileCommandProvider.GetWriteSignatureToTextFileCommand(createSignature.Result, readFileToSign.FilePath);
            commandExecutor.Execute(writeSignatureTofile);
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