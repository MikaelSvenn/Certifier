using System;
using System.Collections;
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
                    CreateOperation = OperationTarget.Key,
                    ShowHelp = true
                };

                Assert.IsFalse(arguments.IsValid);
            }

            [Test]
            public void ShouldBeFalseWhenCreateTargetAndVerifyTargetAreNone()
            {
                arguments = new ApplicationArguments
                {
                    CreateOperation = OperationTarget.None,
                    VerifyOperation = OperationTarget.None
                };

                Assert.IsFalse(arguments.IsValid);
            }

            [Test]
            public void ShouldBeFalseWhenCreateTargetAndVerifyTargetAreNotNone()
            {
                arguments = new ApplicationArguments
                {
                    CreateOperation = OperationTarget.Key,
                    VerifyOperation = OperationTarget.Signature
                };

                Assert.IsFalse(arguments.IsValid);
            }

            [Test]
            public void ShouldBeTrueWhenCreateTargetIsNotNoneAndVerifyTargetIsNone()
            {
                arguments = new ApplicationArguments
                {
                    CreateOperation = OperationTarget.Signature,
                    VerifyOperation = OperationTarget.None
                };

                Assert.IsTrue(arguments.IsValid);
            }

            [Test]
            public void ShouldBeTrueWhenCreateTargetIsNoneAndVerifyTargetIsNotNone()
            {
                arguments = new ApplicationArguments
                {
                    CreateOperation = OperationTarget.None,
                    VerifyOperation = OperationTarget.Key
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
                    CreateOperation = OperationTarget.None
                };

                Assert.IsFalse(arguments.IsCreate);
            }

            [TestCase(OperationTarget.Key, TestName = "Key")]
            [TestCase(OperationTarget.Signature, TestName = "Signature")]
            public void ShouldBeTrueWhenCreateIsNotNone(OperationTarget target)
            {
                arguments = new ApplicationArguments
                {
                    CreateOperation = target
                };

                Assert.IsTrue(arguments.IsCreate);
            }
        }

        [TestFixture]
        public class HasSignature : ApplicationArgumentsTest
        {
            [Test]
            public void ShouldBeTrueWhenSignatureHasContent()
            {
                arguments = new ApplicationArguments
                {
                    Signature = "."
                };

                Assert.IsTrue(arguments.HasSignature);
            }

            [TestCase(null, TestName = "null")]
            [TestCase("", TestName = "empty")]
            [TestCase(" ", TestName = "whitespace")]
            public void ShouldBeFalseWhenSignatureHasNoContent(string signature)
            {
                arguments = new ApplicationArguments
                {
                    Signature = signature
                };

                Assert.IsFalse(arguments.HasSignature);
            }
        }

        [TestFixture]
        public class HasFileOutput : ApplicationArgumentsTest
        {
            [Test]
            public void ShouldBeTrueWhenFileOutputHasContent()
            {
                arguments = new ApplicationArguments
                {
                    FileOutput = "foo"
                };
                
                Assert.IsTrue(arguments.HasFileOutput);
            }
            
            [TestCase(null, TestName = "null")]
            [TestCase("", TestName = "empty")]
            [TestCase(" ", TestName = "whitespace")]
            public void ShouldBeFalseWhenFileOutputHasNoContent(string output)
            {
                arguments = new ApplicationArguments
                {
                    FileOutput = output
                };

                Assert.IsFalse(arguments.HasFileOutput);
            }
        }

        [TestFixture]
        public class HasFileInput : ApplicationArgumentsTest
        {
            [Test]
            public void ShouldBeTrueWhenFileInputHasContent()
            {
                arguments = new ApplicationArguments
                {
                    FileInput = "foobar"
                };
                
                Assert.IsTrue(arguments.HasFileInput);
            }
            
            [TestCase(null, TestName = "null")]
            [TestCase("", TestName = "empty")]
            [TestCase(" ", TestName = "whitespace")]
            public void ShouldBeFalseWhenFileInputHasNoContent(string input)
            {
                arguments = new ApplicationArguments
                {
                    FileInput = input
                };

                Assert.IsFalse(arguments.HasFileInput);
            }
        }
    }
}