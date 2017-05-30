namespace Ui.Console.Command
{
    public interface ICommandWithOutput<T>
    {
        T Out { get; set; }
    }
}