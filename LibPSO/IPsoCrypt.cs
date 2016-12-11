using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibPSO
{
    public interface IPsoCrypt
    {
        void CryptData(UInt32[] data, EncryptionDirection direction);
        byte[] CryptData(byte[] bytes, EncryptionDirection direction);
        void SeekForward(SeekOrigin origin, uint offset);
    }
}
