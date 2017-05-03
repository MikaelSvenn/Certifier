namespace Ui.Console.Command
{
    public class ReadFromTextFileCommand<T> : ICommandWithResult<T>
    {
        public string FilePath { get; set; }
        public string ContentFromFile { get; set; }
        public T Result { get; set; }
    }
}