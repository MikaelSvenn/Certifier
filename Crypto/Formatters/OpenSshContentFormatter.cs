using System.Text.RegularExpressions;

namespace Crypto.Formatters
{
    public class OpenSshContentFormatter
    {
        public virtual string FormatToOpenSshKeyContentLength(string key)
        {
            if (key.Length <= 70)
            {
                return key;
            }
            
            var regex = new Regex(@".{70}");
            return regex.Replace(key, "$&" + "\n");
        }
    }
}