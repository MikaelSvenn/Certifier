using System;
using Core.Interfaces;
using Core.Model;
using Core.SystemWrappers;
using Crypto.Formatters;

namespace Crypto.Providers
{
    public class SshFormattingProvider : ISshFormattingProvider
    {
        // https://tools.ietf.org/html/rfc4253
        // https://tools.ietf.org/html/rfc4716
        // https://tools.ietf.org/html/rfc5656

        private readonly ISshKeyProvider sshKeyProvider;
        private readonly EncodingWrapper encoding;
        private readonly Ssh2ContentFormatter ssh2ContentFormatter;

        public SshFormattingProvider(ISshKeyProvider sshKeyProvider, EncodingWrapper encoding, Ssh2ContentFormatter ssh2ContentFormatter)
        {
            this.sshKeyProvider = sshKeyProvider;
            this.encoding = encoding;
            this.ssh2ContentFormatter = ssh2ContentFormatter;
        }

        public virtual string GetAsOpenSsh(IAsymmetricKey key, string comment)
        {
            if (key.IsPrivateKey)
            {
                throw new InvalidOperationException("Private key cannot be formatted as OpenSSH public key.");
            }
            
            string header;
            string content;
            switch (key.CipherType)
            {
                case CipherType.Ec:
                    VerifyEcCurve(key);
                    header = sshKeyProvider.GetCurveSshHeader(((IEcKey) key).Curve);
                    content = sshKeyProvider.GetEcPublicKeyContent(key);
                    break;
                case CipherType.Rsa:
                    header = "ssh-rsa";
                    content = sshKeyProvider.GetRsaPublicKeyContent(key);
                    break;
                case CipherType.Dsa:
                    header = "ssh-dss";
                    content = sshKeyProvider.GetDsaPublicKeyContent(key);
                    break;
                default:
                    throw new InvalidOperationException("Cipher type not supported for OpenSSH key.");
            }

            return $"{header} {content} {comment}";
        }

        private void VerifyEcCurve(IAsymmetricKey key)
        {
            string curve = ((IEcKey) key).Curve;
            if(!sshKeyProvider.IsSupportedCurve(curve))
            {
                throw new ArgumentException($"Curve {curve} is not supported for SSH key.");
            }
        }
        
        public virtual string GetAsSsh2(IAsymmetricKey key, string comment)
        {
            if (key.IsPrivateKey)
            {
                throw new InvalidOperationException("Private key cannot be formatted as SSH2 public key.");
            }

            int commentLength = encoding.GetBytes(comment).Length;
            if (commentLength > 1024)
            {
                throw new ArgumentException("Comment is too long.");
            }
            
            string contentLine;
            switch (key.CipherType)
            {
                case CipherType.Rsa:
                    contentLine = sshKeyProvider.GetRsaPublicKeyContent(key);
                    break;
                case CipherType.Dsa:
                    contentLine = sshKeyProvider.GetDsaPublicKeyContent(key);
                    break;
                case CipherType.Ec:
                    VerifyEcCurve(key);
                    contentLine = sshKeyProvider.GetEcPublicKeyContent((IEcKey) key);
                    break;
                default:
                    throw new InvalidOperationException("Cipher type not supported for SSH2 key.");
            }

            string content = ssh2ContentFormatter.FormatSsh2KeyContent(contentLine);
            string formattedComment = ssh2ContentFormatter.FormatSsh2Header(comment);
            
            return $"---- BEGIN SSH2 PUBLIC KEY ----{Environment.NewLine}" + 
                   $"Comment: {formattedComment + Environment.NewLine}" + 
                   $"{content + Environment.NewLine}" + 
                   "---- END SSH2 PUBLIC KEY ----";
        }
    }
}