using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibPSO.PsoPatcher
{
    public enum PsoPatchType : uint
    {
        SingleByte = 0,
        SingleHalfWord = 1,
        SingleWord = 2,
        RangeWord = 3,
    }
}
