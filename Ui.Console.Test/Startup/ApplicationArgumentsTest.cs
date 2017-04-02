using NUnit.Framework;
using Ui.Console.Startup;

namespace Ui.Console.Test.Startup
{
    [TestFixture]
    public class ApplicationArgumentsTest
    {
        private ApplicationArguments arguments;

        [TestFixture]
        public class IsValid : ApplicationArgumentsTest
        {
            [Test]
            public void ShouldBeFalseWhenHelpIsShown()
            {
                arguments = new ApplicationArguments
                {
                    Create = OperationTarget.key,
                    ShowHelp = true
                };

                Assert.IsFalse(arguments.IsValid);
            }

            [Test]
            public void ShouldBeFalseWhenCreateTargetAndVerifyTargetAreNone()
            {
                arguments = new ApplicationArguments
                {
                    Create = OperationTarget.none,
                    Verify = OperationTarget.none
                };

                Assert.IsFalse(arguments.IsValid);
            }

            [Test]
            public void ShouldBeFalseWhenCreateTargetAndVerifyTargetAreNotNone()
            {
                arguments = new ApplicationArguments
                {
                    Create = OperationTarget.key,
                    Verify = OperationTarget.signature
                };

                Assert.IsFalse(arguments.IsValid);
            }

            [Test]
            public void ShouldBeTrueWhenCreateTargetIsNotNoneAndVerifyTargetIsNone()
            {
                arguments = new ApplicationArguments
                {
                    Create = OperationTarget.signature,
                    Verify = OperationTarget.none
                };

                Assert.IsTrue(arguments.IsValid);
            }

            [Test]
            public void ShouldBeTrueWhenCreateTargetIsNoneAndVerifyTargetIsNotNone()
            {
                arguments = new ApplicationArguments
                {
                    Create = OperationTarget.none,
                    Verify = OperationTarget.key
                };

                Assert.IsTrue(arguments.IsValid);
            }
        }

    }
}