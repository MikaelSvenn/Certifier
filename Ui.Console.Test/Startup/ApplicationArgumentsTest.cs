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
                    CreateOperation = OperationTarget.key,
                    ShowHelp = true
                };

                Assert.IsFalse(arguments.IsValid);
            }

            [Test]
            public void ShouldBeFalseWhenCreateTargetAndVerifyTargetAreNone()
            {
                arguments = new ApplicationArguments
                {
                    CreateOperation = OperationTarget.none,
                    VerifyOperation = OperationTarget.none
                };

                Assert.IsFalse(arguments.IsValid);
            }

            [Test]
            public void ShouldBeFalseWhenCreateTargetAndVerifyTargetAreNotNone()
            {
                arguments = new ApplicationArguments
                {
                    CreateOperation = OperationTarget.key,
                    VerifyOperation = OperationTarget.signature
                };

                Assert.IsFalse(arguments.IsValid);
            }

            [Test]
            public void ShouldBeTrueWhenCreateTargetIsNotNoneAndVerifyTargetIsNone()
            {
                arguments = new ApplicationArguments
                {
                    CreateOperation = OperationTarget.signature,
                    VerifyOperation = OperationTarget.none
                };

                Assert.IsTrue(arguments.IsValid);
            }

            [Test]
            public void ShouldBeTrueWhenCreateTargetIsNoneAndVerifyTargetIsNotNone()
            {
                arguments = new ApplicationArguments
                {
                    CreateOperation = OperationTarget.none,
                    VerifyOperation = OperationTarget.key
                };

                Assert.IsTrue(arguments.IsValid);
            }
        }

        [TestFixture]
        public class IsCreate : ApplicationArgumentsTest
        {
            [Test]
            public void ShouldBeFalseWhenCreateIsNone()
            {
                arguments = new ApplicationArguments
                {
                    CreateOperation = OperationTarget.none
                };

                Assert.IsFalse(arguments.IsCreate);
            }

            [TestCase(OperationTarget.key, TestName = "Key")]
            [TestCase(OperationTarget.signature, TestName = "Signature")]
            public void ShouldBeTrueWhenCreateIsNotNone(OperationTarget target)
            {
                arguments = new ApplicationArguments
                {
                    CreateOperation = target
                };

                Assert.IsTrue(arguments.IsCreate);
            }
        }

    }
}