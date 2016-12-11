using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibPSO
{
    public static class Helper
    {
        public static UInt16 GetUInt16(IEnumerable<byte> values)
        {
            UInt16 result = 0;
            foreach (var v in values)
            {
                result <<= 8;
                result |= v;
            }
            return result;
        }

        public static UInt32 GetUInt32(IEnumerable<byte> values)
        {
            //if more/less bytes wrong result!
            UInt32 result = 0;
            foreach (var v in values)
            {
                result <<= 8;
                result |= v;
            }
            return result;
        }

        public static UInt32 GetUInt32(byte[] bytes, int offset)
        {
            UInt32 rVal = (UInt32)
                    (
                    bytes[offset + 0] << 8 * 3
                    |
                    bytes[offset + 1] << 8 * 2
                    |
                    bytes[offset + 2] << 8 * 1
                    |
                    bytes[offset + 3] << 8 * 0
                    );
            return rVal;
        }

        public static byte[] GetBytes(UInt32 value)
        {

            return new byte[] 
            {
                (byte)((value >> (8 * 3) & 0xFF)),
                (byte)((value >> (8 * 2) & 0xFF)),
                (byte)((value >> (8 * 1) & 0xFF)),
                (byte)((value >> (8 * 0) & 0xFF)),
                
            };
        }

        public static byte[] GetBytes(UInt16 value)
        {
            return new byte[] 
            {
                (byte)(value >> 8),
                (byte)(value & 0xFF),
            };
        }

        public static IEnumerable<byte> XorCrypt(byte[] message, UInt32 key)
        {
            List<byte> decoded = new List<byte>();
            for (int i = 0; i < message.Length - 3; i += 4)
            {
                yield return (byte)(message[i + 0] ^ (key >> (8 * 3)) & 0xFF);
                yield return (byte)(message[i + 1] ^ (key >> (8 * 2)) & 0xFF);
                yield return (byte)(message[i + 2] ^ (key >> (8 * 1)) & 0xFF);
                yield return (byte)(message[i + 3] ^ (key >> (8 * 0)) & 0xFF);
            }
        }

        public static UInt32 LE32(UInt32 x)
        {
            return (((x >> 24) & 0x00FF) |
                 ((x >> 8) & 0xFF00) |
                 ((x & 0xFF00) << 8) |
                 ((x & 0x00FF) << 24));
        }

        public static UInt16 LE16(UInt16 x)
        {
            return (UInt16)((x >> 8) | (x << 8));
        }
    }
}
