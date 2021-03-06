﻿using Core.Model;
using Core.SystemWrappers;
using Ui.Console.Command;
using Ui.Console.CommandHandler;

namespace Ui.Console.Decorator
{
    public class WriteToFileBase64FormattingDecorator<T> : ICommandHandler<T> where T : WriteFileCommand<Signature>
    {
        private readonly ICommandHandler<T> decoratedCommandHandler;
        private readonly Base64Wrapper base64;
        private readonly EncodingWrapper encoding;

        public WriteToFileBase64FormattingDecorator(ICommandHandler<T> decoratedCommandHandler, Base64Wrapper base64, EncodingWrapper encoding)
        {
            this.decoratedCommandHandler = decoratedCommandHandler;
            this.base64 = base64;
            this.encoding = encoding;
        }

        public void Execute(T command)
        {
            var base64Formatted = base64.ToBase64String(command.Out.Content);
            command.FileContent = encoding.GetBytes(base64Formatted);
            decoratedCommandHandler.Execute(command);
        }
    }
}