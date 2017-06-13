using Ui.Console.Startup;

namespace Ui.Console.Provider
{
    public interface ISignatureCommandActivationProvider
    {
        void CreateSignature(ApplicationArguments arguments);
        void VerifySignature(ApplicationArguments arguments);
    }
}