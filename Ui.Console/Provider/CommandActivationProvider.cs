using System;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
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
        private readonly EncodingWrapper encoding;

        public CommandActivationProvider(ICommandExecutor commandExecutor, RsaKeyCommandProvider rsaKeyCommandProvider, FileCommandProvider fileCommandProvider, SignatureCommandProvider signatureCommandProvider, EncodingWrapper encoding)
        {
            this.commandExecutor = commandExecutor;
            this.rsaKeyCommandProvider = rsaKeyCommandProvider;
            this.fileCommandProvider = fileCommandProvider;
            this.signatureCommandProvider = signatureCommandProvider;
            this.encoding = encoding;
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
            commandExecutor.Execute(readPrivateKeyFromFile);

            byte[] contentToSign;
            if (arguments.HasFileInput)
            {
                ReadFileCommand<byte[]> readFileToSign = fileCommandProvider.GetReadFileCommand<byte[]>(arguments.FileInput);
                commandExecutor.Execute(readFileToSign);  
                contentToSign = readFileToSign.Result;
            }
            else
            {
                contentToSign = encoding.GetBytes(arguments.Input);
            }

            CreateSignatureCommand createSignature = signatureCommandProvider.GetCreateSignatureCommand(readPrivateKeyFromFile.Result, contentToSign);
            commandExecutor.Execute(createSignature);

            if (arguments.HasFileOutput)
            {
                WriteFileCommand<Signature> writeSignatureTofile = fileCommandProvider.GetWriteToFileCommand(createSignature.Result, arguments.FileOutput);
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