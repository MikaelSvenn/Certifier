namespace Ui.Console.Command
{
    public class WriteToFileCommand<T> : FileCommand<T>
    {
        public byte[] FileContent { get; set; }
    }
}