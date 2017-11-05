using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Interfaces;
using Core.SystemWrappers;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Crypto.Providers
{
    public class SshKeyProvider : ISshKeyProvider
    {
        private readonly EncodingWrapper encoding;
        private readonly Base64Wrapper base64;
        private readonly IRsaKeyProvider rsaKeyProvider;
        private readonly IDsaKeyProvider dsaKeyProvider;
        private readonly IEcKeyProvider ecKeyProvider;
        private readonly IEnumerable<string[]> sshSupportedCurves;
        private readonly Dictionary<string, string> sshCurveHeaders;
        private readonly Dictionary<string, string> sshCurveIdentifiers;
        
        public SshKeyProvider(EncodingWrapper encoding, Base64Wrapper base64, IRsaKeyProvider rsaKeyProvider, IDsaKeyProvider dsaKeyProvider, IEcKeyProvider ecKeyProvider)
        {
            this.encoding = encoding;
            this.base64 = base64;
            this.rsaKeyProvider = rsaKeyProvider;
            this.dsaKeyProvider = dsaKeyProvider;
            this.ecKeyProvider = ecKeyProvider;

            sshSupportedCurves = new []
            {
                new []{"curve25519"},
                new []{"P-256", "secp256r1", "prime256v1"},
                new []{"P-384", "secp384r1"},
                new []{"P-521", "secp521r1"}
            };
            
            sshCurveHeaders = new Dictionary<string, string>
            {
                {"curve25519", "ssh-ed25519"},
                {"P-256", "ecdsa-sha2-nistp256"},
                {"P-384", "ecdsa-sha2-nistp384"},
                {"P-521", "ecdsa-sha2-nistp521"}
            };
            
            sshCurveIdentifiers = new Dictionary<string, string>
            {
                {"curve25519", "ed25519"},
                {"P-256", "nistp256"},
                {"P-384", "nistp384"},
                {"P-521", "nistp521"}
            };
        }

        public string GetEcPublicKeyContent(IAsymmetricKey key)
        {
            var ecKey = (IEcKey) key;
            var keyParameters = (ECPublicKeyParameters) PublicKeyFactory.CreateKey(key.Content);
            byte[] header = encoding.GetBytes(sshCurveIdentifiers[ecKey.Curve]);
            byte[] q = keyParameters.Q.GetEncoded();
            
            using (var stream = new MemoryStream())
            {
                stream.Write(LengthAsBytes(header.Length), 0, 4);
                stream.Write(header, 0, header.Length);
                stream.Write(LengthAsBytes(q.Length), 0, 4);
                stream.Write(q, 0, q.Length);
                stream.Flush();
                return base64.ToBase64String(stream.ToArray());
            }
        }

        public string GetRsaPublicKeyContent(IAsymmetricKey key)
        {
            var keyParameters = (RsaKeyParameters) PublicKeyFactory.CreateKey(key.Content);
            byte[] header = encoding.GetBytes("ssh-rsa");
            byte[] e = keyParameters.Exponent.ToByteArray();
            byte[] n = keyParameters.Modulus.ToByteArray();

            using (var stream = new MemoryStream())
            {
                stream.Write(LengthAsBytes(header.Length), 0, 4);
                stream.Write(header, 0, header.Length);
                stream.Write(LengthAsBytes(e.Length), 0, 4);
                stream.Write(e, 0, e.Length);
                stream.Write(LengthAsBytes(n.Length), 0, 4);
                stream.Write(n, 0, n.Length);
                stream.Flush();
                return base64.ToBase64String(stream.ToArray());
            }
        }

        public string GetDsaPublicKeyContent(IAsymmetricKey key)
        {
            var keyParameters = (DsaPublicKeyParameters) PublicKeyFactory.CreateKey(key.Content);
            byte[] header = encoding.GetBytes("ssh-dss");
            byte[] p = keyParameters.Parameters.P.ToByteArray();
            byte[] q = keyParameters.Parameters.Q.ToByteArray();
            byte[] g = keyParameters.Parameters.G.ToByteArray();
            byte[] y = keyParameters.Y.ToByteArray();

            using (var stream = new MemoryStream())
            {
                stream.Write(LengthAsBytes(header.Length), 0, 4);
                stream.Write(header, 0, header.Length);
                stream.Write(LengthAsBytes(p.Length), 0, 4);
                stream.Write(p, 0, p.Length);
                stream.Write(LengthAsBytes(q.Length), 0, 4);
                stream.Write(q, 0, q.Length);
                stream.Write(LengthAsBytes(g.Length), 0, 4);
                stream.Write(g, 0, g.Length);
                stream.Write(LengthAsBytes(y.Length), 0, 4);
                stream.Write(y, 0, y.Length);
                stream.Flush();
                return base64.ToBase64String(stream.ToArray());
            }
        }

        private static byte[] LengthAsBytes(int length)
        {
            byte[] bytes = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }
        
        public bool IsSupportedCurve(string curveName)
        {
            return sshSupportedCurves.Any(c => c.Contains(curveName));
        }

        public string GetCurveSshHeader(string curveName)
        {
            string curve = sshSupportedCurves.Single(c => c.Contains(curveName))
                                             .First();

            return sshCurveHeaders[curve];
        }

        public IAsymmetricKey GetKeyFromSsh(string sshKeyContent)
        {
            byte[] content = base64.FromBase64String(sshKeyContent);
            var headerLengthArray = new byte[4];
            
            Array.Copy(content, 0, headerLengthArray, 0, 4);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(headerLengthArray);
            }

            int headerLength = BitConverter.ToInt32(headerLengthArray, 0);
            var header = new byte[headerLength];
            Array.Copy(content, 4, header, 0, headerLength);
            string headerContent = encoding.GetString(header);

            switch (headerContent)
            {
                case "ssh-rsa":
                    return GetPublicRsaKey(content);
                case "ssh-dss":
                    return GetPublicDsaKey(content);
                case "ed25519":
                case "nistp256":
                case "nistp384":
                case "nistp521":
                    return GetPublicEcKey(content, headerContent);
                default:
                    throw new ArgumentException("SSH key type not supported or the key is corrupt.");
            }
        }

        private IAsymmetricKey GetPublicRsaKey(byte[] content)
        {
            byte[] e;
            byte[] n;
            
            using (var stream = new MemoryStream(content))
            {
                stream.Seek(11, SeekOrigin.Begin);
                e = ReadNextContent(stream);
                n = ReadNextContent(stream);
            }

            return rsaKeyProvider.GetPublicKey(e, n);
        }

        private static byte[] ReadNextContent(MemoryStream stream)
        {
            var contentLengthBytes = new byte[4];
            stream.Read(contentLengthBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(contentLengthBytes);
            }

            int headerLength = BitConverter.ToInt32(contentLengthBytes, 0);
            int contentLength = headerLength;
            
            var contentBytes = new byte[contentLength];
            stream.Read(contentBytes, 0, contentLength);
            return contentBytes;
        }

        private IAsymmetricKey GetPublicDsaKey(byte[] content)
        {
            byte[] p;
            byte[] q;
            byte[] g;
            byte[] y;
            
            using (var stream = new MemoryStream(content))
            {
                stream.Seek(11, SeekOrigin.Begin);
                p = ReadNextContent(stream);
                q = ReadNextContent(stream);
                g = ReadNextContent(stream);
                y = ReadNextContent(stream);
            }

            return dsaKeyProvider.GetPublicKey(p, q, g, y);
        }

        private IAsymmetricKey GetPublicEcKey(byte[] content, string curve)
        {
            byte[] q;
            
            using (var stream = new MemoryStream(content))
            {
                stream.Seek(curve.Length + 4, SeekOrigin.Begin);
                q = ReadNextContent(stream);
            }

            string curveName = sshCurveIdentifiers.Keys.Single(k => sshCurveIdentifiers[k].Equals(curve));
            return ecKeyProvider.GetPublicKey(q, curveName);
        }
    }
}