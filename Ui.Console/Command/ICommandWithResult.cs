namespace Ui.Console.Command
{
    public interface ICommandWithResult<T>
    {
        T Result { get; set; }
    }
}