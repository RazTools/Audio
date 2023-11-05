using System.Text;

namespace Audio.Models.Utils
{
    public static class FNV1
    {
        private const uint PRIME32 = 0x1000193;
        private const uint INIT32 = 0x811C9DC5;
        private const ulong PRIME64 = 0x100000001B3;
        private const ulong INIT64 = 0xCBF29CE484222325;

        public static uint Compute32(string value)
        {
            var lower = value.ToLower();
            var bytes = Encoding.UTF8.GetBytes(lower);
            return ComputeHash32(bytes);
        }

        public static ulong Compute64(string value)
        {
            var lower = value.ToLower();
            var bytes = Encoding.UTF8.GetBytes(lower);
            return ComputeHash64(bytes);
        }
        private static uint ComputeHash32(byte[] data) => (uint)ComputeHash(data, INIT32, PRIME32, uint.MaxValue);
        private static ulong ComputeHash64(byte[] data) => ComputeHash(data, INIT64, PRIME64, ulong.MaxValue);
        private static ulong ComputeHash(byte[] data, ulong init, ulong prime, ulong size)
        {
            var hval = init;
            foreach (var b in data)
            {
                hval = hval * prime & size;
                hval ^= b;
            }
            return hval;
        }
    }
}
