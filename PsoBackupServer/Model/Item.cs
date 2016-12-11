using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsoBackupServer.Model
{
    public class Item
    {
        public UInt16 equipped;
        public UInt16 tech;
        public UInt32 flags;

        public UInt32[] data_l = new UInt32[3];

        public UInt32 item_id;
        public UInt32 data2_l;
    };
}
