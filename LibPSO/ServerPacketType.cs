using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibPSO
{
    public enum ServerPacketType : byte
    {
        None = 0x0,
        Welcome = 0x17,
        Redirect = 0x19,
        MessageBox = 0xD5,
        UpdateCodePacketType = 0xb2,
        Timestamp = 0xB1,
        BlockListType = 0x07,

    }
}
