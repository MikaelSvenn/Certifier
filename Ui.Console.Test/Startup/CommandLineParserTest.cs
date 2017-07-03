using Core.Model;
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
            public void KeyType()
            {
                Assert.AreEqual(CipherType.Rsa, arguments.KeyType);
            }

            [Test]
            public void EncryptionType()
            {
                Assert.AreEqual(KeyEncryptionType.None, arguments.EncryptionType);
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
            public void Input()
            {
                Assert.IsEmpty(arguments.Input);
            }

            [Test]
            public void FileInput()
            {
                Assert.IsEmpty(arguments.FileInput);
            }

            [Test]
            public void FileOutput()
            {
                Assert.IsEmpty(arguments.FileOutput);
            }

            [Test]
            public void Signature()
            {
                Assert.IsEmpty(arguments.Signature);
            }

            [Test]
            public void CreateTarget()
            {
                Assert.AreEqual(OperationTarget.None, arguments.CreateOperation);
            }

            [Test]
            public void VerifyTarget()
            {
                Assert.AreEqual(OperationTarget.None, arguments.VerifyOperation);
            }

            [Test]
            public void Type()
            {
                Assert.AreEqual(ContentType.Pem, arguments.ContentType);
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

            private static string[][] validArguments = {
                new []{"-c", "key", "-b", "2048"},
                new []{"-c", "key", "-k", "dsa", "-b", "2048"},
                new []{"-c", "key", "-k", "rsa", "-e", "pkcs", "-b", "2048"},
                new []{"-c", "signature", "--privatekey", "foo", "-i", "bar"},
                new []{"--privatekey", "foo"},
                new []{"--verify", "signature", "-s", "foobarbaz", "-i", "foofile"},
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
            private static string[][] createKey = {
                new []{"-c", "key"},
                new []{"--create", "key"},
            };

            [Test, TestCaseSource("createKey")]
            public void CreateKey(string[] input)
            {
                var result = commandLineParser.ParseArguments(input);
                Assert.AreEqual(result.CreateOperation, OperationTarget.Key);
            }

            private static string[][] createSignature = {
                new []{"-c", "signature"},
                new []{"--create", "signature"},
            };

            [Test, TestCaseSource("createSignature")]
            public void CreateSignature(string[] input)
            {
                var result = commandLineParser.ParseArguments(input);
                Assert.AreEqual(result.CreateOperation, OperationTarget.Signature);
            }

            private static string[][] verifyKey = {
                new []{"-v", "key"},
                new []{"--verify", "key"},
            };

            [Test, TestCaseSource("verifyKey")]
            public void VerifyKey(string[] input)
            {
                var result = commandLineParser.ParseArguments(input);
                Assert.AreEqual(result.VerifyOperation, OperationTarget.Key);
            }

            private static string[][] verifySignature = {
                new []{"-v", "signature"},
                new []{"--verify", "signature"},
            };

            [Test, TestCaseSource("verifySignature")]
            public void VerifySignature(string[] input)
            {
                var result = commandLineParser.ParseArguments(input);
                Assert.AreEqual(result.VerifyOperation, OperationTarget.Signature);
            }

            private static string[][] keySize = {
                new []{"-b", "2048"},
                new []{"--keysize", "2048"}
            };

            [Test, TestCaseSource("keySize")]
            public void KeySize(string[] input)
            {
                var result = commandLineParser.ParseArguments(input);
                Assert.AreEqual(2048, result.KeySize);
            }

            [Test]
            public void KeyType()
            {
                var result = commandLineParser.ParseArguments(new []{"-k", "ec"});
                Assert.AreEqual(CipherType.Ec, result.KeyType);
            }

            [Test]
            public void EncryptionType()
            {
                var result = commandLineParser.ParseArguments(new []{"-e", "pkcs"});
                Assert.AreEqual(KeyEncryptionType.Pkcs, result.EncryptionType);
            }

            private static string[][] password = {
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

            [Test]
            public void Signature()
            {
                var result = commandLineParser.ParseArguments(new []{"--signature", "foobarbaz"});
                Assert.AreEqual("foobarbaz", result.Signature);
            }
            
            private static string[][] input = {
                new []{"-i", "inputcontent"},
                new []{"--in", "inputcontent"}
            };

            [Test, TestCaseSource("input")]
            public void Input(string[] inputTarget)
            {
                var result = commandLineParser.ParseArguments(inputTarget);
                Assert.AreEqual("inputcontent", result.Input);
            }
            
            private static string[][] fileInput = {
                new []{"-f", @"c:\temp\foo.bar"},
                new []{"--file", @"c:\temp\foo.bar"}
            };

            [Test, TestCaseSource("fileInput")]
            public void FileInput(string[] fileInput)
            {
                var result = commandLineParser.ParseArguments(fileInput);
                Assert.AreEqual(@"c:\temp\foo.bar", result.FileInput);
            }
            
            private static string[][] fileOutput = {
                new []{"-o", "outputcontent"},
                new []{"--out", "outputcontent"}
            };

            [Test, TestCaseSource("fileOutput")]
            public void FileOutput(string[] outputTarget)
            {
                var result = commandLineParser.ParseArguments(outputTarget);
                Assert.AreEqual("outputcontent", result.FileOutput);
            }

            private static string[][] contentType = {
                new []{"-t", "der"},
                new []{"--type", "der"}
            };
            
            [TestCaseSource("contentType")]
            public void Type(string[] contentType)
            {
                var result = commandLineParser.ParseArguments(contentType);
                Assert.AreEqual(ContentType.Der, result.ContentType);
            }
        }
    }
}