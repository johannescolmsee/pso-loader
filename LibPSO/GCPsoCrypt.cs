using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibPSO
{
    public class GCPsoCrypt : IPsoCrypt
    {
        public GCPsoCrypt(UInt32 key)
        {
            this.CreateKeys(key);
        }

        const uint KEYS_ARRAY_SIZE = 521;
        UInt32 seed;
        UInt32[] keys = new UInt32[KEYS_ARRAY_SIZE];
        uint currentKey = 0;

        void MixKeys()
        {
            UInt32 r0, r4;
            this.currentKey = 0;
            int r5ctr = 0;
            int r6ctr = 489;
            int r7ctr = 0;

            while (r6ctr != KEYS_ARRAY_SIZE)
            {
                r0 = keys[r6ctr];
                r6ctr++;
                r4 = keys[r5ctr];
                r0 ^= r4;
                keys[r5ctr] = r0;
                r5ctr++;
            }

            while (r5ctr != KEYS_ARRAY_SIZE)
            {
                r0 = keys[r7ctr];
                r7ctr++;
                r4 = keys[r5ctr];
                r0 ^= r4;
                keys[r5ctr] = r0;
                r5ctr++;
            }
        }

        UInt32 GetNextKey()
        {
            currentKey++;
            if (currentKey == KEYS_ARRAY_SIZE)
            {
                MixKeys();
            }
            return keys[currentKey];
        }

        void CreateKeys(UInt32 seed)
        {
            UInt32 x, y, basekey;
            int source1counter, source2counter, source3counter;
            basekey = 0;

            this.seed = seed;

            
            int targetCounter = 0;
            for (x = 0; x <= 16; x++)
            {
                for (y = 0; y < 32; y++)
                {
                    seed = seed * 0x5D588B65;
                    basekey = basekey >> 1;
                    seed++;
                    if ((seed & 0x80000000) != 0)
                    {
                        basekey = basekey | 0x80000000;
                    }
                    else
                    {
                        basekey = basekey & 0x7FFFFFFF;
                    }
                }
                keys[targetCounter] = basekey;
                targetCounter++;
            }
            source1counter = 0;
            source2counter = 1;
            targetCounter--;
            keys[targetCounter] = ((keys[0] >> 9) ^ (keys[targetCounter] << 23)) ^ keys[15];
            source3counter = targetCounter;
            targetCounter++;
            while (targetCounter != KEYS_ARRAY_SIZE)
            {
                keys[targetCounter] = (keys[source3counter] ^ (((keys[source1counter] << 23) & 0xFF800000) ^ ((keys[source2counter] >> 9) & 0x007FFFFF)));    
                targetCounter++;
                source1counter++;
                source2counter++;
                source3counter++;
            }
            MixKeys();
            MixKeys();
            MixKeys();
            this.currentKey = 520;
        }

        public void CryptData(UInt32[] data, EncryptionDirection direction)
        {
            UInt32 current = 0, key;
            for (int i = 0; i < data.Length; i++)
            {
                key = GetNextKey();
                data[current++] = data[current] ^ Helper.LE32(key);
            }
        }

        public byte[] CryptData(byte[] bytes, EncryptionDirection direction)
        {
            UInt32 key;
            byte[] rval = new byte[bytes.Length];
            for (int firstByteInGroup = 0; firstByteInGroup <= bytes.Length - 4; firstByteInGroup += 4)
            {
                key = GetNextKey();
                UInt32 nextData = Helper.GetUInt32(bytes, firstByteInGroup);
                UInt32 decoded = nextData ^ Helper.LE32(key);
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

            
            
            //{
            //    var maxStep = KEYS_ARRAY_SIZE - this.currentKey;
            //    var step = Math.Min(maxStep, i);
            //    tmpCurrentKey += step;
            //    i -= step;
            //    if (tmpCurrentKey == KEYS_ARRAY_SIZE)
            //    {
            //        MixKeys();
            //        tmpCurrentKey = 0;
            //    }
            //}
            //this.currentKey = tmpCurrentKey;
        }
    }
}
