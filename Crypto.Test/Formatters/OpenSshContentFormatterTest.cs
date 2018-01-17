using System;
using System.Linq;
using Crypto.Formatters;
using NUnit.Framework;

namespace Crypto.Test.Formatters
{
    [TestFixture]
    public class OpenSshContentFormatterTest
    {
        private OpenSshContentFormatter formatter;
        private string rawResult;
        private string[] result;
        
        [SetUp]
        public void SetupContentFormatterTest()
        {
            formatter = new OpenSshContentFormatter();
        }

        [TestFixture]
        public class FormatToOpenSshKeyContentLength : OpenSshContentFormatterTest
        {
            [SetUp]
            public void Setup()
            {
                string header = string.Concat(Enumerable.Repeat("a", 2048));
                rawResult = formatter.FormatToOpenSshKeyContentLength(header);
                result = rawResult.Split(new[] {"\n"}, StringSplitOptions.None);
            }
            
            [Test]
            public void ShouldNotProduceLinesLongerThan70Characters()
            {
                Assert.IsTrue(result.All(line => line.Length < 71));
            }

            [Test]
            public void ShouldNotAppendCharactersToContent()
            {
                Assert.IsTrue(result.All(line => line.EndsWith("a")));
            }
        }
    }
}