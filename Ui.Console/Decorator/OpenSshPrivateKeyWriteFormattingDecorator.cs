using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class OpenSshPrivateKeyWriteFormattingDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<IAsymmetricKeyPair>
    {
        private readonly ICommandHandler<T> decoratedCommand;
        private readonly ISshFormattingProvider sshFormattingProvider;
        private readonly EncodingWrapper encoding;

        public OpenSshPrivateKeyWriteFormattingDecorator(ICommandHandler<T> decoratedCommand, ISshFormattingProvider sshFormattingProvider, EncodingWrapper encoding)
        {
            this.decoratedCommand = decoratedCommand;
            this.sshFormattingProvider = sshFormattingProvider;
            this.encoding = encoding;
        }

        public void Execute(T command)
        {
            if (command.ContentType == ContentType.OpenSsh)
            {
                string sshFormattedKey = sshFormattingProvider.GetAsOpenSshPrivateKey(command.Out, "openssh-key");
                command.FileContent = encoding.GetBytes(sshFormattedKey);
            }
            
            decoratedCommand.Execute(command);
        }
    }
}