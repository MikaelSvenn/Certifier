using System.Text;

namespace Core.SystemWrappers
{
    public class EncodingWrapper
    {
        private readonly Encoding encoding = Encoding.UTF8;

        public virtual byte[] GetBytes(string input)
        {
            return encoding.GetBytes(input);
        }

        public virtual string GetString(byte[] input)
        {
            return encoding.GetString(input);
        }
    }
}