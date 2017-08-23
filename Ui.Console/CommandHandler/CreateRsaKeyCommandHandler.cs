using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;

 namespace Ui.Console.CommandHandler
 {
     public class CreateRsaKeyCommandHandler : ICommandHandler<CreateKeyCommand<RsaKey>>
     {
         private readonly IKeyProvider<RsaKey> rsaKeyProvider;

         public CreateRsaKeyCommandHandler(IKeyProvider<RsaKey> rsaKeyProvider)
         {
             this.rsaKeyProvider = rsaKeyProvider;
         }

         public void Execute(CreateKeyCommand<RsaKey> command)
         {
             command.Result = rsaKeyProvider.CreateKeyPair(command.KeySize);
         }
     }
 }