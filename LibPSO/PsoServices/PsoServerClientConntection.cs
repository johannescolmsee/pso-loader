using LibPSO;
using LibPSO.PsoServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibPSO.PsoServices
{
    public class PsoServerClientConntection : IPsoServerClientConntection
    {
        TcpClient _Client;
        ClientType _ClientType;
        NetworkStream _Stream;
        private IPsoCrypt _ServerCrypt;
        private IPsoCrypt _ClientCrypt;
        private bool _ReadingTaskStarted = false;
        public PsoServerClientConntection(TcpClient client, ClientType clientType)
        {
            this._Client = client;
            this._ClientType = clientType;
        }

        private ObservableCollection<PsoMessage> _Messages = new ObservableCollection<PsoMessage>();
        public ReadOnlyObservableCollection<PsoMessage> Messages
        {
            get { return new ReadOnlyObservableCollection<PsoMessage>(this._Messages); }
        }

        public Task InitConnection(UInt32 serverkey, UInt32 clientkey)
        {
            this._Stream = this._Client.GetStream();
            switch (this._ClientType)
            {
                case ClientType.Gamecube:
                    this._ServerCrypt = new GCPsoCrypt(serverkey);
                    this._ClientCrypt = new GCPsoCrypt(clientkey);
                    break;
                case ClientType.Dreamcast:
                case ClientType.PC:
                    this._ServerCrypt = new PCPsoCrypt(serverkey);
                    this._ClientCrypt = new PCPsoCrypt(clientkey);
                    break;
                default:
                    break;
            }
            var packet = Packets.GetWelcomePacket(serverkey, clientkey, this._ClientType, true);
            return SendMessageToClient(packet, false);
        }

        public async Task SendMessageToClient(byte[] message, bool crypt)
        {
            byte[] messageToSend, messageCrypted = null;
            if (!crypt)
            {
                messageToSend = message;
            }
            else
            {
                messageToSend = this._ServerCrypt.CryptData(message, EncryptionDirection.Encrypt);
                messageCrypted = messageToSend;
            }
            await this._Stream.WriteAsync(messageToSend, 0, messageToSend.Length);
            this._AddMessage(new PsoMessage(Direction.Outgoing, message, messageCrypted));
        }


        public async Task StartReadingTask(CancellationToken cancellationToken)
        {
            this._ReadingTaskStarted = true;
            byte[] buffer = new byte[4096];
            while (true)
            {
                if (!this._Client.Client.Connected)
                {
                    break;
                }
                if (this._Stream != null && this._Stream.DataAvailable)
                {
                    var read = await this._Stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    var readCopy = buffer.Take(read).ToArray();
                    var decoded = this._ClientCrypt.CryptData(readCopy, EncryptionDirection.Decrypt);
                    this._AddMessage(new PsoMessage(Direction.Incoming, readCopy, decoded));
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        public async Task<PsoMessage> ReadMessageFromClient()
        {
            if (!this._ReadingTaskStarted)
            {
                byte[] buffer = new byte[4096];
                var read = await this._Stream.ReadAsync(buffer, 0, buffer.Length);
                var readCopy = buffer.Take(read).ToArray();
                var decoded = this._ClientCrypt.CryptData(readCopy, EncryptionDirection.Decrypt);
                var result = new PsoMessage(Direction.Incoming, readCopy, decoded);
                this._AddMessage(result);
                return result;
            }
            return null;
        }

        public ClientType ClientType
        {
            get { return this._ClientType; }
        }

        private object _AddMessageSync = new object();
        private void _AddMessage(PsoMessage psoMessage)
        {
            lock (_AddMessageSync)
            {
                this._Messages.Add(psoMessage);
            }
        }

        public void Dispose()
        {
            this._Stream.Dispose();
            this._Client.Close();
        }
    }
}
