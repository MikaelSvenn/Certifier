namespace Ui.Console.CommandHandler
{
    public interface ICommandHandler<in T>
    {
        void Excecute(T createKeyCommand);
    }
}