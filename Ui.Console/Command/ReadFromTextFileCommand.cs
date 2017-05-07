namespace Ui.Console.Command
{
    public class ReadFromTextFileCommand<T> : FileCommand<T>
    {
        public string Password { get; set; }
    }
}