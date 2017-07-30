using Ui.Console.Startup;

namespace Ui.Console.Provider
{
    public interface IKeyCommandActivationProvider
    {
        void CreateKeyPair(ApplicationArguments arguments);
        void VerifyKeyPair(ApplicationArguments arguments);
        void ConvertKeyPair(ApplicationArguments arguments);
    }
}