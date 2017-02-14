using Core.Model;
using Core.Services;
using NUnit.Framework;

namespace Core.Test.Services
{
    [TestFixture]
    public class CertificateServiceTest
    {
        private CertificateService certificateService;

        [SetUp]
        public void SetupCertificateServiceTest()
        {
            certificateService = new CertificateService();
        }

        [TestFixture]
        public class CreateCertificateSigningRequest : CertificateServiceTest
        {
            private CertificateSigningRequest csr;

            [SetUp]
            public void Setup()
            {
                var key = new byte[] {0x01};
                var details = new CertificateDetails();

                csr = certificateService.CreateCertificateSigningRequest(key, details);
            }

            [Test]
            public void ShouldCreateValidCsrWithGivenDetails()
            {

            }

            [Test]
            public void ShouldSignCsrWithGivenKey()
            {

            }
        }

        [TestFixture]
        public class SignCertificate : CertificateServiceTest
        {
            private Certificate certificate;

            [SetUp]
            public void Setup()
            {
                var key = new byte[] {0x02};
                var details = new CertificateDetails();
                var csr = certificateService.CreateCertificateSigningRequest(key, details);

                certificate = certificateService.SignCertificate(key, csr);
            }

            [Test]
            public void ShouldCreateValidCertificate()
            {

            }

            [Test]
            public void ShouldSignCreatedCertificateWithGivenKey()
            {

            }
        }
    }
}