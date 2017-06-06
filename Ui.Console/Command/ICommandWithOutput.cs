namespace Ui.Console.Command
{
    public interface ICommandWithOutput<T> : ICommand
    {
        T Out { get; set; }
    }
}