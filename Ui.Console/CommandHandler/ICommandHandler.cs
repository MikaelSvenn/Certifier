namespace Ui.Console.CommandHandler
{
    public interface ICommandHandler<in T>
    {
        void Execute(T command);
    }
}