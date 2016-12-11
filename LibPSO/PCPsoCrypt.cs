using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibPSO
{
    public class PCPsoCrypt : IPsoCrypt
    {
        public PCPsoCrypt(UInt32 key)
        {
            this.CreateKeys(key);
        }
        
        const uint KEYS_ARRAY_SIZE = 57;
        UInt32 seed;
        UInt32[] keys = new UInt32[KEYS_ARRAY_SIZE];
        uint currentKey = 0;


        void MixKeys()
        {
            UInt32 esi, edi, eax, ebp, edx;
            edi = 1;
            edx = 0x18;
            eax = edi;
            while (edx > 0)
            {
                esi = this.keys[eax + 0x1F];
                ebp = this.keys[eax];
                ebp = ebp - esi;
                this.keys[eax] = ebp;
                eax++;
                edx--;
            }
            edi = 0x19;
            edx = 0x1F;
            eax = edi;
            while (edx > 0)
            {
                esi = this.keys[eax - 0x18];
                ebp = this.keys[eax];
                ebp = ebp - esi;
                this.keys[eax] = ebp;
                eax++;
                edx--;
            }
        }

        void CreateKeys(UInt32 val)
        {
            UInt32 esi, ebx, edi, eax, edx, var1;
            esi = 1;
            ebx = val;
            edi = 0x15;

            this.seed = val;

            this.keys[56] = ebx;
            this.keys[55] = ebx;
            while (edi <= 0x46E)
            {
                eax = edi;
                var1 = eax / 55;
                edx = eax - (var1 * 55);
                ebx = ebx - esi;
                edi = edi + 0x15;
                this.keys[edx] = esi;
                esi = ebx;
                ebx = this.keys[edx];
            }
            MixKeys();
            MixKeys();
            MixKeys();
            MixKeys();
            this.currentKey = 56;
        }

        UInt32 GetNextKey()
        {
            UInt32 re;
            if (this.currentKey == 56)
            {
                MixKeys();
                this.currentKey = 1;
            }
            re = this.keys[this.currentKey];
            this.currentKey++;
            return re;
        }

        public void CryptData(UInt32[] data, EncryptionDirection direction)
        {
            for (UInt32 x = 0; x < data.Length; x++)
            {
                UInt32 tmp = data[x];
                tmp = Helper.LE32(tmp) ^ GetNextKey();
                data[x] = Helper.LE32(tmp);
            }
        }

        public byte[] CryptData(byte[] bytes, EncryptionDirection direction)
        {
            byte[] rval = new byte[bytes.Length];
            for (int firstByteInGroup = 0; firstByteInGroup <= bytes.Length - 4; firstByteInGroup += 4)
            {
                UInt32 tmp = Helper.GetUInt32(bytes, firstByteInGroup);
                UInt32 decoded = Helper.LE32(tmp) ^ GetNextKey();
                int i = 0;
                foreach (var b in Helper.GetBytes(decoded))
                {
                    rval[firstByteInGroup + i++] = b;
                }
            }
            return rval;
        }


        public void SeekForward(SeekOrigin origin, uint offset)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    CreateKeys(this.seed);
                    break;
                case SeekOrigin.Current:
                    break;
                default:
                    throw new Exception("Direction not supported.");
            }
            for (int i = 0; i < offset; i++)
            {
                GetNextKey();
            }
        }
    }
}
