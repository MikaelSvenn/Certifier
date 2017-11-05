using System;
using System.Text.RegularExpressions;

namespace Crypto.Formatters
{
    public class Ssh2ContentFormatter
    {
        public virtual string FormatToSsh2Header(string header)
        {
            if (header.Length <= 71)
            {
                return header;
            }
            
            var regex = new Regex(@".{71}");
            return regex.Replace(header, "$&" + $"\\{Environment.NewLine}");
        }

        public virtual string FormatToSsh2KeyContent(string key)
        {
            if (key.Length <= 72)
            {
                return key;
            }
            
            var regex = new Regex(@".{72}");
            return regex.Replace(key, "$&" + Environment.NewLine);
        }
    }
}