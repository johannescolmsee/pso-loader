using LibPSO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PsoBackupServer.PsoReadMem
{
    public class PsoReadMemProgram
    {
        public static byte[] GetPsoReadMemProgram(UInt32 address)
        {
            var readMemCode = _GetReadMemCode();
            return
                readMemCode
                .Concat(
                    Helper.GetBytes(address)
                ).ToArray();
        }

        private static byte[] _GetReadMemCode()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "PsoBackupServer.PsoReadMem.pso_readmem.bin";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
