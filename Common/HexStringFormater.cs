using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class HexStringFormater
    {

        public static string GetHexText(byte[] p, int offset, int bytesPerLine, int numLines)
        {
            var bytes = p
                .Skip(offset);
            if (numLines > 0)
            {
                bytes = bytes.Take((int)(numLines * bytesPerLine));
            }
            var lines = bytes
                .Select((b, ix) => new { Byte = b, Index = ix })
                .GroupBy(x => x.Index / bytesPerLine)
                .Select(g => new { LineAddress = g.Key * bytesPerLine + offset, Bytes = g.Select(x => x.Byte).ToArray() })
                .Select(x => _GetLine(x.LineAddress, x.Bytes, 16));
            return String.Join(Environment.NewLine, lines);
        }

        private static string _GetLine(int lineAddress, byte[] bytes, int bytesPerLine)
        {
            var adddoubleSpaceCount = bytesPerLine - bytes.Length;
            return String.Format(@"{0:x8}: {1}    {2}",
                lineAddress,
                String.Join(" ", bytes.Select(x => string.Format(@"{0:x2}", x)).Concat(Enumerable.Range(0, adddoubleSpaceCount).Select(x => "  "))),
                _GetText(bytes)
                );
        }

        private static string _GetText(byte[] bytes)
        {
            return new string(bytes.Select(x => x >= 0x20 && x <= 127 ? (char)x : '.').ToArray());
        }

    }
}
