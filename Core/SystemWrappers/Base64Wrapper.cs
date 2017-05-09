namespace Core.SystemWrappers
{
    public class Base64Wrapper
    {
        public string ToBase64String(byte[] content)
        {
            return System.Convert.ToBase64String(content);
        }

        public byte[] FromBase64String(string content)
        {
            return System.Convert.FromBase64String(content);
        }
    }
}