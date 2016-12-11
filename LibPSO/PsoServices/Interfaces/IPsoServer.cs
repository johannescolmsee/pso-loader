using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibPSO.PsoServices.Interfaces
{
    public interface IPsoServer
    {
        ReadOnlyObservableCollection<string> Messages { get; }
        Task<IPsoServerClientConntection> AwaitConnectionAsync(int port, System.Net.IPAddress address, ClientType clientType);
    }
}
