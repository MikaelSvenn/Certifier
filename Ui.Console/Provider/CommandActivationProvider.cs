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

            WriteFileCommand<IAsymmetricKey> writePrivateKeyToFile = fileCommandProvider.GetWriteToFileCommand<IAsymmetricKey>(createKeyCommand.Result.PrivateKey, arguments.PrivateKeyPath);
            WriteFileCommand<IAsymmetricKey> writePublicKeyToFile = fileCommandProvider.GetWriteToFileCommand<IAsymmetricKey>(createKeyCommand.Result.PublicKey, arguments.PublicKeyPath);
            commandExecutor.ExecuteSequence(new []{writePrivateKeyToFile, writePublicKeyToFile});
        }

        public void CreateSignature(ApplicationArguments arguments)
        {
            ReadKeyFromFileCommand readPrivateKeyFromFile = fileCommandProvider.GetReadPrivateKeyFromFileCommand(arguments.PrivateKeyPath, arguments.Password);
            ReadFileCommand<byte[]> readFileToSign = fileCommandProvider.GetReadFileCommand<byte[]>(arguments.Input);
            commandExecutor.ExecuteSequence(new dynamic[]{readPrivateKeyFromFile, readFileToSign});

            CreateSignatureCommand createSignature = signatureCommandProvider.GetCreateSignatureCommand(readPrivateKeyFromFile.Result, readFileToSign.Result);
            commandExecutor.Execute(createSignature);

            if (arguments.HasOutput)
            {
                WriteFileCommand<Signature> writeSignatureTofile = fileCommandProvider.GetWriteToFileCommand(createSignature.Result, arguments.Output);
                commandExecutor.Execute(writeSignatureTofile);
                return;
            }

            WriteToStdOutCommand<Signature> writeSignatureToStdOut = fileCommandProvider.GetWriteToStdOutCommand<Signature>(createSignature.Result);
            commandExecutor.Execute(writeSignatureToStdOut);
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