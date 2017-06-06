using System;
using Core.SystemWrappers;
using Ui.Console.Startup;

namespace Ui.Console
{
    public class Certifier
    {
        public static void Main(string[] commandLineArguments)
        {
            AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;

            var container = Bootstrap.Initialize(commandLineArguments);
            var arguments = container.GetInstance<ApplicationArguments>();
            var activator = container.GetInstance<CommandActivator>();

            if (arguments.ShowHelp || !arguments.IsValid)
            {
                var console = container.GetInstance<ConsoleWrapper>();
                console.WriteLine("HELP!");
                return;
            }

            if (arguments.IsCreate)
            {
                activator.Create[arguments.CreateOperation](arguments);
                return;
            }

            activator.Verify[arguments.VerifyOperation](arguments);
        }

        private static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs arguments)
        {
            string message = ((Exception) arguments.ExceptionObject).Message;
            System.Console.WriteLine($"{Environment.NewLine} Error: {message}");
            Environment.Exit(1);
        }
    }
}