using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsoBackupServer
{
    public class PsoSaveGame
    {
        public String CharName { get; set; }
        public byte[] DataReadFromRam { get; set; }
        public byte[] DataSentByClient { get; set; }

        public DateTime SaveTime { get; set; }
    }
}
