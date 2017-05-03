using System;
using System.Collections.Generic;
using SimpleInjector;
using Ui.Console.CommandHandler;

namespace Ui.Console.Startup
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly Container container;

        public CommandExecutor(Container container)
        {
            this.container = container;
        }

        public void Execute(dynamic command)
        {
            Type commandType = command.GetType();
            Type commandHandlerType = typeof(ICommandHandler<>);
            Type commandHandlerInstanceType = commandHandlerType.MakeGenericType(commandType);

            var commandHander = (dynamic)container.GetInstance(commandHandlerInstanceType);
            commandHander.Execute(command);
        }

        public void ExecuteSequence(IEnumerable<dynamic> commands)
        {
            foreach (var command in commands)
            {
                Execute(command);
            }
        }
    }
}