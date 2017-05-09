namespace Ui.Console.Command
{
    public class WriteToTextFileCommand<T> : FileCommand<T>, ITextFileCommand
    {
        public string FileContent { get; set; }
    }
}