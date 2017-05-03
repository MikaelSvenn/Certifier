using System.Linq;
using NUnit.Framework;
using SimpleInjector;
using Ui.Console.Command;
using Ui.Console.CommandHandler;
using Ui.Console.Startup;

namespace Ui.Console.Test.Startup
{
    [TestFixture]
    public class CommandExecutorTest
    {
        private CommandExecutor executor;

        [SetUp]
        public void SetupCommandExecutorTest()
        {
            var container = new Container();
            int index = 0;

            container.Register<ICommandHandler<TestCommand>>(() => new TestCommandHandler(index++));
            executor = new CommandExecutor(container);
        }

        [Test]
        public void ExecuteShouldExecuteCorrespondingCommandHandler()
        {
            var command = new TestCommand(7);

            executor.Execute(command);
            Assert.AreEqual(0, command.Result);
        }

        [Test]
        public void ExecuteSequenceShouldExecuteGivenOrdersInGivenSequence()
        {
            var commands = Enumerable.Range(0, 10)
                .Select(index => new TestCommand(index))
                .ToList();

            executor.ExecuteSequence(commands);

            Assert.IsTrue(commands.All(c => c.Result == c.Index));
        }
    }

    public class TestCommand : ICommandWithResult<int>
    {
        public TestCommand(int index)
        {
            Index = index;
        }

        public int Index { get;}
        public int Result { get; set; }
    }

    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        private readonly int index;

        public TestCommandHandler(int index)
        {
            this.index = index;
        }

        public void Execute(TestCommand command)
        {
            command.Result = index;
        }
    }
}