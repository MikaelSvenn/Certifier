using System;
using System.Collections.Generic;
using Ui.Console.Provider;

namespace Ui.Console.Startup
{
    public class CommandActivator
    {
        public CommandActivator(ICommandActivationProvider activationProvider)
        {
            Create = new Dictionary<OperationTarget, Action<ApplicationArguments>>
            {
                {OperationTarget.none, arguments => { throw new InvalidOperationException("Create operation not specified."); }},
                {OperationTarget.key, activationProvider.CreateKey},
                {OperationTarget.signature, activationProvider.CreateSignature}
            };


            Verify = new Dictionary<OperationTarget, Action<ApplicationArguments>>
            {
                {OperationTarget.none, arguments => { throw new InvalidOperationException("Verify operation not specified."); }},
                {OperationTarget.key, activationProvider.VerifyKeyPair},
                {OperationTarget.signature, activationProvider.VerifySignature}
            };
        }

        public Dictionary<OperationTarget, Action<ApplicationArguments>> Create { get; }
        public Dictionary<OperationTarget, Action<ApplicationArguments>> Verify { get; }
    }
}