using Core.Model;

namespace Core.Services
{
    public class CertificateService : ICertificateService
    {
        public CertificateSigningRequest CreateCertificateSigningRequest(byte[] privateKey, CertificateDetails certificateDetails)
        {
            throw new System.NotImplementedException();
        }

        public Certificate SignCertificate(byte[] privateKey, CertificateSigningRequest certificateSigningRequest)
        {
            throw new System.NotImplementedException();
        }
    }
}