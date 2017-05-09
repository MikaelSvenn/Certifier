namespace Ui.Console.Command
{
    public class ReadFromTextFileCommand<T> : FileCommand<T>, ITextFileCommand
    {
        public string Password { get; set; }
        public string FileContent { get; set; }
    }
}