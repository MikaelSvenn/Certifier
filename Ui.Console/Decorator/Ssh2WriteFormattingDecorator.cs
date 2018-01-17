using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class Ssh2WriteFormattingDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommand;
        private readonly ISshFormattingProvider sshFormattingProvider;
        private readonly EncodingWrapper encoding;

        public Ssh2WriteFormattingDecorator(ICommandHandler<T> decoratedCommand, ISshFormattingProvider sshFormattingProvider, EncodingWrapper encoding)
        {
            this.decoratedCommand = decoratedCommand;
            this.sshFormattingProvider = sshFormattingProvider;
            this.encoding = encoding;
        }

        public void Execute(T command)
        {
            if (command.ContentType == ContentType.Ssh2)
            {
                string sshFormattedKey = sshFormattingProvider.GetAsSsh2PublicKey(command.Out, "ssh2-key");
                command.FileContent = encoding.GetBytes(sshFormattedKey);
            }
            
            decoratedCommand.Execute(command);
        }
    }
}