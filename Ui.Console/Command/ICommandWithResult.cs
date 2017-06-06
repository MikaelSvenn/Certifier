namespace Ui.Console.Command
{
    public interface ICommandWithResult<T> : ICommand
    {
        T Result { get; set; }
    }
}