namespace Ui.Console.Command
{
    public class WriteToTextFileCommand<T>
    {
        public string Destination { get; set; }
        public T Content { get; set; }
        public string ContentToFile { get; set; }
    }
}