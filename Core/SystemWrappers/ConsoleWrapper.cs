namespace Core.SystemWrappers
{
    public class ConsoleWrapper
    {
        public virtual void WriteLine(string input)
        {
            System.Console.WriteLine(input);
        }
    }
}