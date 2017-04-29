using System.Collections.Generic;

namespace Ui.Console.Startup
{
    public interface ICommandExecutor
    {
        void Execute(dynamic command);
        void ExecuteSequence(IEnumerable<object> commands);
    }
}