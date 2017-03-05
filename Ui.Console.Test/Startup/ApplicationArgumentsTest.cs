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
                    Create = CreateTarget.key,
                    ShowHelp = true
                };

                Assert.IsFalse(arguments.IsValid);
            }

            [Test]
            public void ShouldBeFalseWhenCreateTargetAndVerifyTargetAreNone()
            {
                arguments = new ApplicationArguments
                {
                    Create = CreateTarget.none,
                    Verify = VerifyTarget.none
                };

                Assert.IsFalse(arguments.IsValid);
            }

            [Test]
            public void ShouldBeFalseWhenCreateTargetAndVerifyTargetAreNotNone()
            {
                arguments = new ApplicationArguments
                {
                    Create = CreateTarget.key,
                    Verify = VerifyTarget.signature
                };

                Assert.IsFalse(arguments.IsValid);
            }

            [Test]
            public void ShouldBeTrueWhenCreateTargetIsNotNoneAndVerifyTargetIsNone()
            {
                arguments = new ApplicationArguments
                {
                    Create = CreateTarget.signature,
                    Verify = VerifyTarget.none
                };

                Assert.IsTrue(arguments.IsValid);
            }

            [Test]
            public void ShouldBeTrueWhenCreateTargetIsNoneAndVerifyTargetIsNotNone()
            {
                arguments = new ApplicationArguments
                {
                    Create = CreateTarget.none,
                    Verify = VerifyTarget.key
                };

                Assert.IsTrue(arguments.IsValid);
            }
        }

    }
}