using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class SshWriteFormattingDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommand;
        private readonly ISshFormattingProvider sshFormattingProvider;
        private readonly EncodingWrapper encoding;

        public SshWriteFormattingDecorator(ICommandHandler<T> decoratedCommand, ISshFormattingProvider sshFormattingProvider, EncodingWrapper encoding)
        {
            this.decoratedCommand = decoratedCommand;
            this.sshFormattingProvider = sshFormattingProvider;
            this.encoding = encoding;
        }

        public void Execute(T command)
        {
            string sshFormattedKey;
            switch (command.ContentType)
            {
                case ContentType.OpenSsh:
                    sshFormattedKey = sshFormattingProvider.GetAsOpenSsh(command.Out, "converted key");
                    command.FileContent = encoding.GetBytes(sshFormattedKey);
                    break;
                case ContentType.Ssh2:
                    sshFormattedKey = sshFormattingProvider.GetAsSsh2(command.Out, "converted key");
                    command.FileContent = encoding.GetBytes(sshFormattedKey);
                    break;
                default:
                    command.FileContent = command.Out.Content;
                    break;
            }
            
            decoratedCommand.Execute(command);
        }
    }
}