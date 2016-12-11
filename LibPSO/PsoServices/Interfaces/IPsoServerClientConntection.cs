using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibPSO.PsoServices.Interfaces
{
    public interface IPsoServerClientConntection : IDisposable
    {
        ClientType ClientType { get; }

        ReadOnlyObservableCollection<PsoMessage> Messages { get; }

        Task InitConnection(UInt32 serverkey, UInt32 clientkey);

        Task StartReadingTask(CancellationToken cancellationToken);

        Task<PsoMessage> ReadMessageFromClient();

        Task SendMessageToClient(byte[] message, bool crypt);
    }
}
