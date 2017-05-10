using SimpleInjector;

namespace Ui.Console.Provider
{
    public static class ContainerProvider
    {
        private static Container Container { get; set; }
        public static Container GetContainer()
        {
            return Container ?? (Container = new Container());
        }

        public static void ClearContainer()
        {
            Container?.Dispose();
            Container = null;
        }
    }
}