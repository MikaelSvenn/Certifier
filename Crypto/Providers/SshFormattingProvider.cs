using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Xml;
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
        private readonly Base64Wrapper base64;

        private readonly IEnumerable<string> supportedSshHeaders;
        
        public SshFormattingProvider(ISshKeyProvider sshKeyProvider, EncodingWrapper encoding, Ssh2ContentFormatter ssh2ContentFormatter, Base64Wrapper base64)
        {
            this.sshKeyProvider = sshKeyProvider;
            this.encoding = encoding;
            this.ssh2ContentFormatter = ssh2ContentFormatter;
            this.base64 = base64;

            supportedSshHeaders = new[] {"---- BEGIN SSH2 PUBLIC", "ssh-rsa ", "ssh-dss ", "ssh-ed25519 ", "ecdsa-sha2-nistp256 ", "ecdsa-sha2-nistp384 ", "ecdsa-sha2-nistp521 "};
        }

        public string GetAsOpenSsh(IAsymmetricKey key, string comment)
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
        
        public string GetAsSsh2(IAsymmetricKey key, string comment)
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
                    contentLine = sshKeyProvider.GetEcPublicKeyContent(key);
                    break;
                default:
                    throw new InvalidOperationException("Cipher type not supported for SSH2 key.");
            }

            string content = ssh2ContentFormatter.FormatToSsh2KeyContent(contentLine);
            string formattedComment = ssh2ContentFormatter.FormatToSsh2Header(comment);
            
            return $"---- BEGIN SSH2 PUBLIC KEY ----{Environment.NewLine}" + 
                   $"Comment: {formattedComment + Environment.NewLine}" + 
                   $"{content + Environment.NewLine}" + 
                   "---- END SSH2 PUBLIC KEY ----";
        }

        public IAsymmetricKey GetAsDer(string sshKey)
        {
            string[] contentLines;
            if (sshKey.StartsWith("---- BEGIN SSH2 PUBLIC KEY ----"))
            {
                contentLines = sshKey.Split(new[] {"\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries);
                IEnumerable<string> keyContent = contentLines.Where(line => base64.IsBase64(line));
                string key = string.Concat(keyContent);
                
                return sshKeyProvider.GetKeyFromSsh(key);
            }
            
            contentLines = sshKey.Split(' ');
            return sshKeyProvider.GetKeyFromSsh(contentLines[1]);
        }

        public bool IsSshKey(string sshKey) => supportedSshHeaders.Any(sshKey.StartsWith);
    }
}