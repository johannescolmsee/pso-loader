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
    public class UpdateCodePackageResultPackage
    {
        public PacketHeader Header { get; set; }
        public UInt32 ReturnValue { get; set; }
        public UInt32 Unknown { get; set; }

        public byte[] GetBytes(ClientType clientType)
        {
            return
                this.Header.GetBytes(clientType)
                .Concat(Helper.GetBytes(Helper.LE32(this.ReturnValue)))
                .Concat(Helper.GetBytes(Helper.LE32(this.Unknown)))
                .ToArray();
        }

        public static UpdateCodePackageResultPackage FromBytes(byte[] bytes, ClientType type)
        {
            var res = new UpdateCodePackageResultPackage();
            res.Header = PacketHeader.FromBytes(bytes, type);
            res.ReturnValue = Helper.LE32(Helper.GetUInt32(bytes.Skip(4).Take(4)));
            res.Unknown = Helper.LE32(Helper.GetUInt32(bytes.Skip(8).Take(4)));
            return res;
        }
    }
}
