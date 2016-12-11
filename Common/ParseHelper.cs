using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class ParseHelper
    {
        public static IEnumerable<byte> GetByteArrayEnumFromHexString(string arg)
        {
            var enumerator = arg.ToUpper().GetEnumerator();
            int count = 0;
            byte value = 0;
            while (enumerator.MoveNext())
            {
                char current = enumerator.Current;
                var numericVal = current - '0';
                var alphaval = current - 'A';
                byte charVal;
                if (numericVal >= 0 && numericVal <= 9)
                {
                    charVal = (byte)numericVal;
                }
                else if (alphaval >= 0 && alphaval <= 5)
                {
                    charVal = (byte)(alphaval + 10);
                }
                else
                {
                    continue;
                }
                value |= (byte)(charVal << ((1 - (count & 1)) * 4));
                count++;
                if ((count & 1) == 0)
                {
                    yield return value;
                    value = 0;
                }
            }
        }
    }
}
