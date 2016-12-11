using LibPSO.PsoServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LibPSO.PacketDefinitions
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UpdateCodePackage
    {
        public const UInt16 SIZE_WITHOUT_CODE = 2 * 2 + 4 * 4 + 4 * 5 + 6 * 2;
        public PacketHeader Header { get; set; }

        //seems to be "end of code + 0x10 --> relative zu diesem ding ist die struct "hinter dem code" addressiert; must NOT be null for code to be called
        public UInt32 OffsetOffsetSavedEntryAddress { get; set; }//LE32 

        public UInt32 UnknownHeader1 { get; set; }//LE32 //0x00000080 --> LE32'd 0x80000000 (start of gamecube cached RAM)
        public UInt32 UnknownHeader2 { get; set; }//LE32 //0x02000000 --> LE32'd 0x00000002

        public UInt32 EntryOffset { get; set; }//Entrypoint offset relative to this data (so - if entrypoint is 0x00 in code - 0x00000004 is the correct value
        public byte[] Code { get; set; }

        //TODO: determine LE state of all the following (probably not LE'd)
        public UInt32 OffsetEntryPointCalculationCounter { get; set; } //Offsest to a counter about the entry-addresses
        public UInt32 EntryPointCalculationCounter { get; set; }//should be 1 (must be > 0 for address to be calculated (otherwise *could* be the absolute addresse maybe))
        public UInt32 UnknownFooter1 { get; set; }//0
        public UInt32 UnknownFooter2 { get; set; }//0
        public UInt32 OffsetSavedEntryAddress { get; set; }//this will be added to the return value of the method calculating (and saving) the "entrypoints" -> should be 0

        public UInt16 OffsetCalculationEntry0 { get; set; }
        public UInt16 OffsetCalculationEntry1 { get; set; }
        public UInt16 OffsetCalculationEntry2 { get; set; }
        public UInt16 OffsetCalculationEntry3 { get; set; }
        public UInt16 OffsetCalculationEntry4 { get; set; }
        public UInt16 OffsetCalculationEntry5 { get; set; }

        public byte[] GetBytes(ClientType clientType)
        {
            return 
                Header.GetBytes(clientType)
                .Concat(Helper.GetBytes(Helper.LE32(OffsetOffsetSavedEntryAddress)))
                .Concat(Helper.GetBytes(Helper.LE32(UnknownHeader1)))
                .Concat(Helper.GetBytes(Helper.LE32(UnknownHeader2)))
                .Concat(Helper.GetBytes(EntryOffset))
                .Concat(Code)
                .Concat(Helper.GetBytes(OffsetEntryPointCalculationCounter))
                .Concat(Helper.GetBytes(EntryPointCalculationCounter))
                .Concat(Helper.GetBytes(UnknownFooter1))
                .Concat(Helper.GetBytes(UnknownFooter2))
                .Concat(Helper.GetBytes(OffsetSavedEntryAddress))
                .Concat(Helper.GetBytes(OffsetCalculationEntry0))
                .Concat(Helper.GetBytes(OffsetCalculationEntry1))
                .Concat(Helper.GetBytes(OffsetCalculationEntry2))
                .Concat(Helper.GetBytes(OffsetCalculationEntry3))
                .Concat(Helper.GetBytes(OffsetCalculationEntry4))
                .Concat(Helper.GetBytes(OffsetCalculationEntry5))
                .ToArray();
        }
    }
}
