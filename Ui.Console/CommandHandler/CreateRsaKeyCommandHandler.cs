using Core.Interfaces;
using Core.Model;
using Ui.Console.Command;

 namespace Ui.Console.CommandHandler
 {
     public class CreateRsaKeyCommandHandler : ICommandHandler<CreateRsaKeyCommand>
     {
         private readonly IAsymmetricKeyProvider<RsaKey> rsaKeyProvider;

         public CreateRsaKeyCommandHandler(IAsymmetricKeyProvider<RsaKey> rsaKeyProvider)
         {
             this.rsaKeyProvider = rsaKeyProvider;
         }

         public void Excecute(CreateRsaKeyCommand createKeyCommand)
         {
             createKeyCommand.Result = rsaKeyProvider.CreateKeyPair(createKeyCommand.KeySize);
         }
     }
 }