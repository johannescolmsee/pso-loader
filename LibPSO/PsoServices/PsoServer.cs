using LibPSO;
using LibPSO.PsoServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LibPSO.PsoServices
{
    public class PsoServer : IPsoServer
    {
        public async Task<IPsoServerClientConntection> AwaitConnectionAsync(int port, System.Net.IPAddress address, ClientType clientType)
        {
            TcpListener server = null;
            try
            {
                server = new TcpListener(address, port);
                server.Start();
                this._Messages.Add(String.Format("{0:G}: Accepting new clients.", DateTime.Now));
                var client = await server.AcceptTcpClientAsync();
                this._Messages.Add(String.Format("{0:G}: Accepted new client.", DateTime.Now));
                return new PsoServerClientConntection(client, clientType);
            }
            catch (SocketException e)
            {
                this._Messages.Add(String.Format("{0:G}: SocketException: {1}", DateTime.Now, e));
                throw;
            }
            finally
            {
                this._Messages.Add(String.Format("{0:G}: Stopped listening.", DateTime.Now));
                server.Stop();
            }
        }

        private ObservableCollection<string> _Messages = new ObservableCollection<string>();
        public ReadOnlyObservableCollection<string> Messages
        {
            get { return new ReadOnlyObservableCollection<string>(this._Messages); }
        }
    }
}
