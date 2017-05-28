using System;
using Core.Interfaces;
using Core.Model;
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

            WriteToFileCommand<IAsymmetricKey> writePrivateKeyToFile = fileCommandProvider.GetWriteKeyToFileCommand(createKeyCommand.Result.PrivateKey, arguments.PrivateKeyPath);
            WriteToFileCommand<IAsymmetricKey> writePublicKeyToFile = fileCommandProvider.GetWriteKeyToFileCommand(createKeyCommand.Result.PublicKey, arguments.PublicKeyPath);
            commandExecutor.ExecuteSequence(new []{writePrivateKeyToFile, writePublicKeyToFile});
        }

        public void CreateSignature(ApplicationArguments arguments)
        {
            ReadKeyFromFileCommand readPrivateKeyFromFile = fileCommandProvider.GetReadKeyFromFileCommand(arguments.PrivateKeyPath, arguments.Password);
            ReadFromFileCommand readFileToSign = fileCommandProvider.GetReadFromFileCommand(arguments.Input);
            commandExecutor.ExecuteSequence(new dynamic[]{readPrivateKeyFromFile, readFileToSign});

            CreateSignatureCommand createSignature = signatureCommandProvider.GetCreateSignatureCommand(readPrivateKeyFromFile.Result, readFileToSign.Result);
            commandExecutor.Execute(createSignature);

            WriteToFileCommand<Signature> writeSignatureTofile = fileCommandProvider.GetWriteSignatureToFileCommand(createSignature.Result, readFileToSign.FilePath);
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