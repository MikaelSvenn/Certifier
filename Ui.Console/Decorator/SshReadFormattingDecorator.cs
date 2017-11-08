using Core.Interfaces;
using Core.SystemWrappers;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class SshReadFormattingDecorator<T> : ICommandHandler<T> where T : ReadFileCommand<IAsymmetricKey>
    {
        private readonly ICommandHandler<T> decoratedCommand;
        private readonly ISshFormattingProvider sshFormattingProvider;
        private readonly EncodingWrapper encoding;

        public SshReadFormattingDecorator(ICommandHandler<T> decoratedCommand, ISshFormattingProvider sshFormattingProvider, EncodingWrapper encoding)
        {
            this.decoratedCommand = decoratedCommand;
            this.sshFormattingProvider = sshFormattingProvider;
            this.encoding = encoding;
        }

        public void Execute(T command)
        {
            decoratedCommand.Execute(command);
            if (command.Result != null)
            {
                return;
            }
            
            string decodedContent = encoding.GetString(command.FileContent);
            if (!sshFormattingProvider.IsSshKey(decodedContent))
            {
                return;
            }
            
            command.Result = sshFormattingProvider.GetAsDer(decodedContent);
       }
    }
}