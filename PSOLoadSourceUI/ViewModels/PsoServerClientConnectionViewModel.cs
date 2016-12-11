namespace PSOLoadSourceUI.ViewModels
{
    using Catel.Data;
    using Catel.MVVM;
    using LibPSO.PsoPatcher;
    using LibPSO.PsoServices.Interfaces;
    using LibPSO.PsoVersionDetector;
    using System.Threading;
    using System.Linq;
    using System.Threading.Tasks;
    using System;
    using LibPSO;

    public class PsoServerClientConnectionViewModel : ViewModelBase
    {
        Catel.Services.IOpenFileService _OpenFileService;
        Catel.Services.IMessageService _MessageService;
        private CancellationTokenSource _ReadingThreadCancellationTokenSource;
        public PsoServerClientConnectionViewModel(IPsoServerClientConntection client, Catel.Services.IOpenFileService openFileService, Catel.Services.IMessageService messageService)
        {
            this._OpenFileService = openFileService;
            this._MessageService = messageService;
            this.Client = client;
            this.SendMessageTaskCommand = new TaskCommand(this._ExecuteSendMessageTaskCommand, this._CanExecuteSendMessageTaskCommand);
            this.LoadMessageToSendTaskCommand = new TaskCommand(this._ExecuteLoadMessageToSendTaskCommand, this._CanExecuteLoadMessageToSendTaskCommand);
            this.CreateUpdateCodeMessageTaskCommand = new TaskCommand(this._ExecuteCreateUpdateCodeMessageTaskCommand, this._CanExecuteCreateUpdateCodeMessageTaskCommand);
            this.CreatePatchMessageTaskCommand = new TaskCommand(this._ExecuteCreatePatchMessageTaskCommand, this._CanExecuteCreatePatchMessageTaskCommand);
            this.CreateRedirectMessageTaskCommand = new TaskCommand(this._ExecuteCreateRedirectMessageTaskCommand, this._CanExecuteCreateRedirectMessageTaskCommand);
            this.CreateVersionCheckMessageTaskCommand = new TaskCommand(this._ExecuteCreateVersionCheckMessageTaskCommand, this._CanExecuteCreateVersionCheckMessageTaskCommand);
            this.UpdatePacketFlagsCommand = new Command(this._ExecuteUpdatePacketFlagsCommand, this._CanExecuteUpdatePacketFlagsCommand);

        }

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await this.Client.InitConnection(0x4f52554b, 0x49524f4d);
            this._ReadingThreadCancellationTokenSource = new CancellationTokenSource();
            var t = this.Client.StartReadingTask(this._ReadingThreadCancellationTokenSource.Token);
        }

        protected override async Task CloseAsync()
        {
            if (this._ReadingThreadCancellationTokenSource != null)
            {
                this._ReadingThreadCancellationTokenSource.Cancel();
            }
            this.Client.Dispose();
            await base.CloseAsync();
        }

        [Model]
        public IPsoServerClientConntection Client
        {
            get
            {
                return this.GetValue<IPsoServerClientConntection>(ClientProperty);
            }
            set
            {
                this.SetValue(ClientProperty, value);
            }
        }
        public static readonly PropertyData ClientProperty = RegisterProperty<PsoServerClientConnectionViewModel, IPsoServerClientConntection>((x) => x.Client);

        public byte[] MessageToSend
        {
            get
            {
                return this.GetValue<byte[]>(MessageToSendProperty);
            }
            set
            {
                this.SetValue(MessageToSendProperty, value);
            }
        }
        public static readonly PropertyData MessageToSendProperty = RegisterProperty<PsoServerClientConnectionViewModel, byte[]>((x) => x.MessageToSend);

        public bool DoCryptMessage
        {
            get
            {
                return this.GetValue<bool>(DoCryptMessageProperty);
            }
            set
            {
                this.SetValue(DoCryptMessageProperty, value);
            }
        }
        public static readonly PropertyData DoCryptMessageProperty = RegisterProperty<PsoServerClientConnectionViewModel, bool>((x) => x.DoCryptMessage, true);

        public byte CodeMessageFlags
        {
            get
            {
                return this.GetValue<byte>(CodeMessageFlagsProperty);
            }
            set
            {
                this.SetValue(CodeMessageFlagsProperty, value);
            }
        }
        public static readonly PropertyData CodeMessageFlagsProperty = RegisterProperty<PsoServerClientConnectionViewModel, byte>((x) => x.CodeMessageFlags, 0);

        public TaskCommand SendMessageTaskCommand { get; private set; }

        private bool _CanExecuteSendMessageTaskCommand()
        {
            return this.MessageToSend != null && this.MessageToSend.Length > 0;
        }

        private async Task _ExecuteSendMessageTaskCommand()
        {
            var message = this.MessageToSend;
            var doCrypt = this.DoCryptMessage;

            if (message != null && message.Length > 0)
            {
                await this.Client.SendMessageToClient(message, doCrypt);
            }
        }

        public TaskCommand LoadMessageToSendTaskCommand { get; private set; }


        private bool _CanExecuteLoadMessageToSendTaskCommand()
        {
            return true;
        }

        private async Task _ExecuteLoadMessageToSendTaskCommand()
        {
            Exception exception = null;
            try
            {
                var newSource = await _TryLoad();
                if (newSource != null)
                {
                    this.MessageToSend = newSource;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            if (exception != null)
            {
                await this._MessageService.ShowErrorAsync(exception);
            }
        }

        public Command UpdatePacketFlagsCommand
        {
            get
            {
                return this.GetValue<Command>(UpdatePacketFlagsCommandProperty);
            }
            set
            {
                this.SetValue(UpdatePacketFlagsCommandProperty, value);
            }
        }
        public static readonly PropertyData UpdatePacketFlagsCommandProperty = RegisterProperty<PsoServerClientConnectionViewModel, Command>((x) => x.UpdatePacketFlagsCommand);


        private bool _CanExecuteUpdatePacketFlagsCommand()
        {
            return this.MessageToSend != null && this.MessageToSend.Length >= 4;
        }

        private void _ExecuteUpdatePacketFlagsCommand()
        {
            var bytes = this.MessageToSend;
            bytes[1] = this.CodeMessageFlags;
            this.MessageToSend = null;
            this.MessageToSend = bytes;
        }

        public TaskCommand CreateUpdateCodeMessageTaskCommand { get; private set; }

        private bool _CanExecuteCreateUpdateCodeMessageTaskCommand()
        {
            return true;
        }

        private async Task _ExecuteCreateUpdateCodeMessageTaskCommand()
        {
            Exception exception = null;
            try
            {
                var codeBytes = await _TryLoad();
                if (codeBytes != null)
                {
                    var codePackageBytes = LibPSO.Packets.GetUpdateCodePacket(codeBytes, this.Client.ClientType, this.CodeMessageFlags);
                    this.MessageToSend = codePackageBytes;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            if (exception != null)
            {
                await this._MessageService.ShowErrorAsync(exception);
            }
        }

        private async Task<byte[]> _TryLoad()
        {

            var ofs = this._OpenFileService;
            if (ofs.DetermineFile())
            {
                using (var fs = System.IO.File.OpenRead(ofs.FileName))
                {
                    fs.Seek(0, System.IO.SeekOrigin.End);
                    var size = fs.Position;
                    fs.Seek(0, System.IO.SeekOrigin.Begin);
                    var buffer = new byte[size];
                    await fs.ReadAsync(buffer, 0, (int)size);
                    return buffer;
                }
            }
            else
            {
                return null;
            }
        }

        public TaskCommand CreatePatchMessageTaskCommand { get; private set; }

        private bool _CanExecuteCreatePatchMessageTaskCommand()
        {
            return true;
        }

        private async Task _ExecuteCreatePatchMessageTaskCommand()
        {
            Exception exception = null;
            try
            {
                var codeBytes = await _TryCreatePatchMessage();
                if (codeBytes != null)
                {
                    var codePackageBytes = LibPSO.Packets.GetUpdateCodePacket(codeBytes, this.Client.ClientType, this.CodeMessageFlags);
                    this.MessageToSend = codePackageBytes;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            if (exception != null)
            {
                await this._MessageService.ShowErrorAsync(exception);
            }
        }

        private Task<byte[]> _TryCreatePatchMessage()
        {
            return Task.Factory.StartNew(() =>
                {
                    var ofs = this._OpenFileService;
                    if (ofs.DetermineFile())
                    {
                        using (var fs = System.IO.File.OpenRead(ofs.FileName))
                        {
                            var psopatch = PsoPatchDefinition.FromXml(fs);
                            return psopatch.GetPatchProgram();
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            );
        }


        public TaskCommand CreateRedirectMessageTaskCommand { get; private set; }

        private bool _CanExecuteCreateRedirectMessageTaskCommand()
        {
            return true;
        }

        private async Task _ExecuteCreateRedirectMessageTaskCommand()
        {
            Exception exception = null;
            try
            {
                var messageBytes = await _TryCreateRedirectMessage();
                this.MessageToSend = messageBytes;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            if (exception != null)
            {
                await this._MessageService.ShowErrorAsync(exception);
            }
        }

        private Task<byte[]> _TryCreateRedirectMessage()
        {
            return Task.Factory.StartNew(() =>
                {
                    var ofs = this._OpenFileService;
                    if (ofs.DetermineFile())
                    {
                        using (var fs = System.IO.File.OpenRead(ofs.FileName))
                        {
                            var psopatch = PsoPatchDefinition.FromXml(fs);
                            return psopatch.Redirect.GetRedirectPacket(ClientType.Gamecube);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            );
        }

        public TaskCommand CreateVersionCheckMessageTaskCommand { get; private set; }

        private bool _CanExecuteCreateVersionCheckMessageTaskCommand()
        {
            return true;
        }

        private async Task _ExecuteCreateVersionCheckMessageTaskCommand()
        {
            Exception exception = null;
            try
            {
                var codeBytes = await _TryCreateVersionCheck();
                if (codeBytes != null)
                {
                    var codePackageBytes = LibPSO.Packets.GetUpdateCodePacket(codeBytes, this.Client.ClientType, this.CodeMessageFlags);
                    this.MessageToSend = codePackageBytes;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            if (exception != null)
            {
                await this._MessageService.ShowErrorAsync(exception);
            }
        }

        private Task<byte[]> _TryCreateVersionCheck()
        {
            return Task.Factory.StartNew(() =>
                {
                    var ofs = this._OpenFileService;
                    if (ofs.DetermineFile())
                    {
                        using (var fs = System.IO.File.OpenRead(ofs.FileName))
                        {
                            var psoVersionDef = PsoVersionDetectionDefinition.FromXml(fs);
                            return psoVersionDef.GetPsoVersionDetectionProgram();
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            );
        }

        public override string Title { get { return "View model title"; } }
    }
}
