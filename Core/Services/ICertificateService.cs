using Core.Model;

namespace Core.Services
{
    public interface ICertificateService
    {
        CertificateSigningRequest CreateCertificateSigningRequest(byte[] privateKey, CertificateDetails certificateDetails);
        Certificate SignCertificate(byte[] privateKey, CertificateSigningRequest certificateSigningRequest);
    }
}