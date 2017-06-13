using System;
using System.Collections.Generic;
using Ui.Console.Provider;

namespace Ui.Console.Startup
{
    public class CommandActivator
    {
        public CommandActivator(IKeyCommandActivationProvider keyCommandActivationProvider, ISignatureCommandActivationProvider signatureCommandActivationProvider)
        {
            Create = new Dictionary<OperationTarget, Action<ApplicationArguments>>
            {
                {OperationTarget.None, arguments => { throw new InvalidOperationException("Create operation not specified."); }},
                {OperationTarget.Key, keyCommandActivationProvider.CreateKeyPair},
                {OperationTarget.Signature, signatureCommandActivationProvider.CreateSignature}
            };


            Verify = new Dictionary<OperationTarget, Action<ApplicationArguments>>
            {
                {OperationTarget.None, arguments => { throw new InvalidOperationException("Verify operation not specified."); }},
                {OperationTarget.Key, keyCommandActivationProvider.VerifyKeyPair},
                {OperationTarget.Signature, signatureCommandActivationProvider.VerifySignature}
            };
        }

        public Dictionary<OperationTarget, Action<ApplicationArguments>> Create { get; }
        public Dictionary<OperationTarget, Action<ApplicationArguments>> Verify { get; }
    }
}