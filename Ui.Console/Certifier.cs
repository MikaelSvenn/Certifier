using Ui.Console.Startup;

namespace Ui.Console
{
    internal class Certifier
    {
        public static void Main(string[] commandLineArguments)
        {
            var container = Bootstrap.Initialize(commandLineArguments);
            var arguments = container.GetInstance<ApplicationArguments>();

            if (arguments.ShowHelp)
            {
                ShowHelp();
                return;
            }

            if (!arguments.IsValid)
            {
                System.Console.WriteLine("Invalid arguments specified. Type -h for help.");
                return;
            }

            
        }

        public static void ShowHelp()
        {
            System.Console.WriteLine("HELP!");
        }
    }
}