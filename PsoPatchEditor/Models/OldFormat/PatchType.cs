using LibPSO.PsoPatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PsoPatchEditor.Models.OldFormat
{
    public enum PatchType : uint
    {
        Byte = PsoPatchType.SingleByte,
        HalfWord = PsoPatchType.SingleHalfWord,
        Word = PsoPatchType.SingleWord,
    }
}
