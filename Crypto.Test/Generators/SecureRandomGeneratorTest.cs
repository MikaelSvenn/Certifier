using System.Collections.Generic;
using Crypto.Generators;
using NUnit.Framework;

namespace Crypto.Test.Generators
{
    [TestFixture]
    public class SecureRandomGeneratorTest
    {
        private SecureRandomGenerator randomGenerator;

        [SetUp]
        public void SetupSecureRandomGeneratorTest()
        {
            randomGenerator = new SecureRandomGenerator();
        }

        [TestCase(1000, 1024, TestName = "Should create unique random number")]
        public void GenerateRandom(int amount, int randomLenght)
        {
            var results = new List<byte[]>();

            for (var i = 0; i < amount; i++)
            {
                var result = new byte[randomLenght];
                randomGenerator.NextBytes(result);
                results.Add(result);
            }

            CollectionAssert.AllItemsAreUnique(results);
        }
    }
}