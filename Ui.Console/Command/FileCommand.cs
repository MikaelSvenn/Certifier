namespace Ui.Console.Command
{
    public abstract class FileCommand<T> : ICommandWithResult<T>
    {
        public string FilePath { get; set; }
        public T Result { get; set; }
    }
}