using NUnit.Framework;
using Ui.Console.Startup;

namespace Ui.Console.Test.Startup
{
    [TestFixture]
    public class CommandLineParserTest
    {
        private CommandLineParser commandLineParser;

        [SetUp]
        public void SetupCommandLineParserTest()
        {
            commandLineParser = new CommandLineParser();
        }

        [TestFixture]
        public class ShouldParseDefaultArgumentsWith : CommandLineParserTest
        {
            private ApplicationArguments arguments;

            [SetUp]
            public void Setup()
            {
                arguments = commandLineParser.ParseArguments(null);
            }

            [Test]
            public void KeySize()
            {
                Assert.AreEqual(4096, arguments.KeySize);
            }

            [Test]
            public void Password()
            {
                Assert.IsEmpty(arguments.Password);
            }

            [Test]
            public void PrivateKeyPath()
            {
                Assert.IsEmpty(arguments.PrivateKeyPath);
            }

            [Test]
            public void PublicKeyPath()
            {
                Assert.IsEmpty(arguments.PublicKeyPath);
            }

            [Test]
            public void DataPath()
            {
                Assert.IsEmpty(arguments.DataPath);
            }

            [Test]
            public void Signature()
            {
                Assert.IsEmpty(arguments.Signature);
            }

            [Test]
            public void CreateTarget()
            {
                Assert.AreEqual(Console.Startup.CreateTarget.none, arguments.Create);
            }

            [Test]
            public void VerifyTarget()
            {
                Assert.AreEqual(Console.Startup.VerifyTarget.none, arguments.Verify);
            }
        }

        [TestFixture]
        public class ShowHelpShould : CommandLineParserTest
        {
            [Test]
            public void BeIndicatedWhenNoParametersAreGiven()
            {
                var result = commandLineParser.ParseArguments(null);
                Assert.IsTrue(result.ShowHelp);
            }

            [TestCase("-?", TestName = "-?")]
            [TestCase("-h", TestName = "-h")]
            [TestCase("-help", TestName = "-help")]
            public void BeIndicatedWhenHelpIsSpecified(string input)
            {
                var result = commandLineParser.ParseArguments(new[] {input});
                Assert.IsTrue(result.ShowHelp);
            }

            [Test]
            public void BeIndicatedWhenUnknownArgumentsAreGiven()
            {
                var result = commandLineParser.ParseArguments(new[] {"-foo", "17"});
                Assert.IsTrue(result.ShowHelp);
            }

            private static string[][] validArguments = new[]
            {
                new []{"-c", "key", "-k", "2048"},
                new []{"-c", "signature", "--privatekey", "foo", "-f", "bar"},
                new []{"--privatekey", "foo"},
                new []{"--verify", "signature", "-s", "foobarbaz", "-f", "foofile"},

            };

            [Test, TestCaseSource("validArguments")]
            public void NotBeIndicatedWhenAnyValidArgumentIsSpecified(string[] input)
            {
                var result = commandLineParser.ParseArguments(input);
                Assert.IsFalse(result.ShowHelp);
            }
        }

        [TestFixture]
        public class ShouldParseGivenParameters : CommandLineParserTest
        {
            private static string[][] createKey = new[]
            {
                new []{"-c", "key"},
                new []{"--create", "key"},
            };

            [Test, TestCaseSource("createKey")]
            public void CreateKey(string[] input)
            {
                var result = commandLineParser.ParseArguments(input);
                Assert.AreEqual(result.Create, CreateTarget.key);
            }

            private static string[][] createSignature = new[]
            {
                new []{"-c", "signature"},
                new []{"--create", "signature"},
            };

            [Test, TestCaseSource("createSignature")]
            public void CreateSignature(string[] input)
            {
                var result = commandLineParser.ParseArguments(input);
                Assert.AreEqual(result.Create, CreateTarget.signature);
            }

            private static string[][] verifyKey = new[]
            {
                new []{"-v", "key"},
                new []{"--verify", "key"},
            };

            [Test, TestCaseSource("verifyKey")]
            public void VerifyKey(string[] input)
            {
                var result = commandLineParser.ParseArguments(input);
                Assert.AreEqual(result.Verify, VerifyTarget.key);
            }

            private static string[][] verifySignature = new[]
            {
                new []{"-v", "signature"},
                new []{"--verify", "signature"},
            };

            [Test, TestCaseSource("verifySignature")]
            public void VerifySignature(string[] input)
            {
                var result = commandLineParser.ParseArguments(input);
                Assert.AreEqual(result.Verify, VerifyTarget.signature);
            }

            private static string[][] keySize = new[]
            {
                new []{"-k", "2048"},
                new []{"--keysize", "2048"}
            };

            [Test, TestCaseSource("keySize")]
            public void KeySize(string[] input)
            {
                var result = commandLineParser.ParseArguments(input);
                Assert.AreEqual(2048, result.KeySize);
            }

            private static string[][] password = new[]
            {
                new []{"-p", "foobar"},
                new []{"--password", "foobar"}
            };

            [Test, TestCaseSource("password")]
            public void Password(string[] input)
            {
                var result = commandLineParser.ParseArguments(input);
                Assert.AreEqual("foobar", result.Password);
            }

            [Test]
            public void PrivateKeyPath()
            {
                var result = commandLineParser.ParseArguments(new []{"--privatekey", @"c:\temp\private"});
                Assert.AreEqual(@"c:\temp\private", result.PrivateKeyPath);
            }

            [Test]
            public void PublicKeyPath()
            {
                var result = commandLineParser.ParseArguments(new []{"--publickey", @"c:\temp\public"});
                Assert.AreEqual(@"c:\temp\public", result.PublicKeyPath);
            }

            private static string[][] filePath = new[]
            {
                new []{"-f", @"c:\temp\file"},
                new []{"--file", @"c:\temp\file"}
            };

            [Test, TestCaseSource("filePath")]
            public void DataPath(string[] input)
            {
                var result = commandLineParser.ParseArguments(input);
                Assert.AreEqual(@"c:\temp\file", result.DataPath);
            }

            [Test]
            public void Signature()
            {
                var result = commandLineParser.ParseArguments(new []{"--signature", "foobarbaz"});
                Assert.AreEqual("foobarbaz", result.Signature);
            }
        }
    }
}