using LibPSO.PsoServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibPSO.PacketDefinitions
{
    public class MenuResultPackage
    {
        public PacketHeader Header { get; set; }
        public UInt32 MenuId { get; set; }
        public UInt32 ItemId { get; set; }

        public static MenuResultPackage FromBytes(byte[] bytes, ClientType type)
        {
            var res = new MenuResultPackage();
            res.Header = PacketHeader.FromBytes(bytes, type);
            res.MenuId = Helper.LE32(Helper.GetUInt32(bytes.Skip(4).Take(4)));
            res.ItemId = Helper.LE32(Helper.GetUInt32(bytes.Skip(8).Take(4)));
            return res;
        }
    }
}
