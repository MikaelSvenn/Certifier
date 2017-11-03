using System;
using System.Collections.Generic;
using System.Linq;
using Crypto.Formatters;
using NUnit.Framework;

namespace Crypto.Test.Formatters
{
    [TestFixture]
    public class Ssh2ContentFormatterTest
    {
        private Ssh2ContentFormatter formatter;
        private string rawResult;
        private string[] result;
        
        [SetUp]
        public void SetupContentFormatterTest()
        {
            formatter = new Ssh2ContentFormatter();
        }

        [TestFixture]
        public class FormatSsh2Header : Ssh2ContentFormatterTest
        {
            [SetUp]
            public void Setup()
            {
                string header = string.Concat(Enumerable.Repeat("a", 1024));
                rawResult = formatter.FormatSsh2Header(header);
                result = rawResult.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            }

            [Test]
            public void ShouldNotProduceLinesLongerThan72Characters()
            {
                Assert.IsTrue(result.All(line => line.Length < 73));
            }

            [Test]
            public void ShouldAppendBackslashToEachSplitHeaderLine()
            {
                IEnumerable<string> allButLast = result.Reverse().Skip(1);
                Assert.IsTrue(allButLast.All(line => line.EndsWith("\\")));
            }

            [Test]
            public void ShouldNotAppendBackslashToLastHeaderLine()
            {
                Assert.IsFalse(result.Last().EndsWith("\\"));
            }
        }

        [TestFixture]
        public class FormatSsh2KeyContent : Ssh2ContentFormatterTest
        {
            [SetUp]
            public void Setup()
            {
                string header = string.Concat(Enumerable.Repeat("a", 2048));
                rawResult = formatter.FormatSsh2KeyContent(header);
                result = rawResult.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            }
            
            [Test]
            public void ShouldNotProduceLinesLongerThan72Characters()
            {
                Assert.IsTrue(result.All(line => line.Length < 73));
            }

            [Test]
            public void ShouldNotAppendCharactersToContent()
            {
                Assert.IsTrue(result.All(line => line.EndsWith("a")));
            }
        }
    }
}