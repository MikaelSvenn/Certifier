using Ui.Console.Startup;

namespace Ui.Console.Provider
{
    public interface ICommandActivationProvider
    {
        void CreateKey(ApplicationArguments arguments);
        void CreateSignature(ApplicationArguments arguments);
        void VerifyKeyPair(ApplicationArguments arguments);
        void VerifySignature(ApplicationArguments arguments);
    }
}