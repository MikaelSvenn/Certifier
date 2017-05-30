namespace Ui.Console.Command
{
    public class WriteToStdOutCommand<T> : ICommandWithOutput<T>
    {
        public string ContentToStdOut { get; set; }
        public T Out { get; set; }
    }
}