using System;

namespace Core.SystemWrappers
{
    public class Base64Wrapper
    {
        public virtual string ToBase64String(byte[] content) => Convert.ToBase64String(content);
        public virtual byte[] FromBase64String(string content) => Convert.FromBase64String(content);

        public virtual bool IsBase64(string content)
        {
            try
            {
                Convert.FromBase64String(content);
                return true;
            }
            catch(FormatException)
            {
                return false;
            }
        }
    }
}