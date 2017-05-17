using System;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class FilePathValidationDecorator<T, TFile> : ICommandHandler<T> where T : FileCommand<TFile>
    {
        private readonly ICommandHandler<T> decoratedCommandHandler;

        public FilePathValidationDecorator(ICommandHandler<T> decoratedCommandHandler)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
        }

        public void Execute(T command)
        {
            if (string.IsNullOrWhiteSpace(command.FilePath))
            {
                throw new ArgumentException($"Target file is required.");
            }

            decoratedCommandHandler.Execute(command);
        }
    }
}