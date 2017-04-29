using Ui.Console.Startup;

namespace Ui.Console
{
    internal class Certifier
    {
        public static void Main(string[] commandLineArguments)
        {
            var container = Bootstrap.Initialize(commandLineArguments);
            var arguments = container.GetInstance<ApplicationArguments>();
            var activator = container.GetInstance<CommandActivator>();

            if (arguments.ShowHelp || !arguments.IsValid)
            {
                ShowHelp();
                return;
            }

            if (arguments.IsCreate)
            {
                activator.Create[arguments.CreateOperation](arguments);
                return;
            }

            activator.Verify[arguments.VerifyOperation](arguments);
        }

        public static void ShowHelp()
        {
            System.Console.WriteLine("HELP!");
        }
    }
}