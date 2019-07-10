using System;
using System.Linq;

namespace Albar.AssistantAssignment.Algorithm.Utilities
{
    public static class ByteConverter
    {
        public static byte[] GetByte(byte size, int value)
        {
            var bytes = new byte[size];
            var remain = value;
            var pos = 0;
            do
            {
                bytes[pos] = (byte) (remain % 256);
                remain /= 256;
                pos++;
            } while (remain > 0);

            return bytes.Reverse().ToArray();
        }

        public static int ToInt32(byte[] bytes)
        {
            if (bytes.Length > 4) throw new Exception("Byte size out of range");
            return bytes.Reverse().Select((t, i) => t * (int) Math.Pow(256, i)).Sum();
        }

        public static string ToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes);
        }
    }
}