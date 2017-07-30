using System;
using System.Collections;
using System.Net.Mime;
using System.Security.Policy;
using NUnit.Framework;
using Ui.Console.Startup;

namespace Ui.Console.Test.Startup
{
    [TestFixture]
    public class ApplicationArgumentsTest
    {
        private ApplicationArguments arguments;
      
        [TestFixture]
        public class IsValidOperationTest : ApplicationArgumentsTest
        {
            [TestFixture]
            public class ShouldBeTrueWhen : IsValidOperationTest
            {
                [SetUp]
                public void Setup()
                {
                    arguments = new ApplicationArguments
                    {
                        CreateOperation = OperationTarget.None,
                        VerifyOperation = OperationTarget.None,
                        IsConvertOperation = false
                    };
                }
                
                [TestCase(OperationTarget.Key, ExpectedResult = true)]
                [TestCase(OperationTarget.Signature, ExpectedResult = true)]
                public bool CreateOperationIsNotNone(OperationTarget target)
                {
                    arguments.CreateOperation = target;
                    return arguments.IsValidOperation;
                }

                [TestCase(OperationTarget.Key, ExpectedResult = true)]
                [TestCase(OperationTarget.Signature, ExpectedResult = true)]
                public bool VerifyOperationIsNotNone(OperationTarget target)
                {
                    arguments.VerifyOperation = target;
                    return arguments.IsValidOperation;
                }

                [Test]
                public void IsConvertOperationIsNotFalse()
                {
                    arguments.IsConvertOperation = true;
                    Assert.IsTrue(arguments.IsValidOperation);
                }
            }

            [TestFixture]
            public class ShouldBeFalseWhen : IsValidOperationTest
            {
                [SetUp]
                public void Setup()
                {
                    arguments = new ApplicationArguments
                    {
                        CreateOperation = OperationTarget.Key,
                        VerifyOperation = OperationTarget.None,
                        IsConvertOperation = false
                    };
                }

                [TestCase(OperationTarget.Key, OperationTarget.Key, ExpectedResult = false)]
                [TestCase(OperationTarget.Key, OperationTarget.Signature, ExpectedResult = false)]
                [TestCase(OperationTarget.Signature, OperationTarget.Key, ExpectedResult = false)]
                [TestCase(OperationTarget.Signature, OperationTarget.Signature, ExpectedResult = false)]
                public bool CreateAndVerifyOperationsAreNotNone(OperationTarget create, OperationTarget verify)
                {
                    arguments.CreateOperation = create;
                    arguments.VerifyOperation = verify;
                    return arguments.IsValidOperation;
                }

                [Test]
                public void CreateAndVerifyOperationsAreNoneAndIsConvertIsFalse()
                {
                    arguments.CreateOperation = OperationTarget.None;
                    arguments.IsConvertOperation = false;
                    Assert.IsFalse(arguments.IsValidOperation);
                }
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