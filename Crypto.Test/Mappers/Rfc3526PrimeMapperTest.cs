using System;
using Crypto.Generators;
using Crypto.Mappers;
using NUnit.Framework;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Crypto.Test.Mappers
{
    [TestFixture]
    public class Rfc3526PrimeMapperTest
    {
        private Rfc3526PrimeMapper mapper;
        private SecureRandom random;
        
        [SetUp]
        public void Setup()
        {
            mapper = new Rfc3526PrimeMapper();
            random = new SecureRandomGenerator().Generator;
        }

        [TestCase(2048, TestName = "2048 bit key")]
        [TestCase(3072, TestName = "3072 bit key")]
        [TestCase(4096, TestName = "4096 bit key")]
        [TestCase(6144, TestName = "6144 bit key")]
        [TestCase(8192, TestName = "8192 bit key")]
        public void ShouldReturnPrimeWithGenerator(int keySize)
        {
            var parameters = mapper.GetParametersByKeySize(keySize);
            
            //Single iteration is enough; the content is prime unless altered by formatting.
            Assert.IsTrue(Primes.IsMRProbablePrime(parameters.Prime, random, 1));
            Assert.AreEqual(keySize, parameters.Prime.BitLength);
            Assert.AreEqual(2, parameters.Generator.IntValue);
        }

        [Test]
        public void ShouldThrowExceptionWhenNoMatchingPrimeIsFound()
        {
            Assert.Throws<ArgumentException>(() => mapper.GetParametersByKeySize(5500));
        }
    }
}