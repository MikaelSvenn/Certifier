namespace Ui.Console.Command
{
    public class ReadFileCommand<T> : ICommandWithResult<T>
    {
        public T Result { get; set; }
        public string FilePath { get; set; }
        public byte[] FileContent { get; set; }
    }
}