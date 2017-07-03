using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;

 namespace Ui.Console.CommandHandler
 {
     public class CreateRsaKeyCommandHandler : ICommandHandler<CreateRsaKeyCommand>
     {
         private readonly IKeyProvider<RsaKey> rsaKeyProvider;

         public CreateRsaKeyCommandHandler(IKeyProvider<RsaKey> rsaKeyProvider)
         {
             this.rsaKeyProvider = rsaKeyProvider;
         }

         public void Execute(CreateRsaKeyCommand createKeyCommand)
         {
             createKeyCommand.Result = rsaKeyProvider.CreateKeyPair(createKeyCommand.KeySize);
         }
     }
 }