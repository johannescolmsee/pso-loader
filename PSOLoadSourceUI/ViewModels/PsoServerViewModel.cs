namespace PSOLoadSourceUI.ViewModels
{
    using Catel.Data;
    using Catel.MVVM;
    using Catel.Services;
    using LibPSO.PsoServices.Interfaces;
    using PSOLoadSourceUI.Views;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class PsoServerViewModel : ViewModelBase
    {
        IUIVisualizerService _UIVisualizerService;
        public PsoServerViewModel(IPsoServer psoserver, Catel.Services.IUIVisualizerService uiVisualizerService)
        {
            this.PsoServer = psoserver;
            this._UIVisualizerService = uiVisualizerService;
            this.StartServerTaskCommand = new TaskCommand(this._ExecuteStartServerTaskCommand, this._CanExecuteStartServerTaskCommand);
        }

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            this.ClientTypes = Enum.GetValues(typeof(ClientType)).Cast<ClientType>().ToArray();
            this.SelectedClientType = this.ClientTypes.First();
        }

        protected override async Task CloseAsync()
        {
            await base.CloseAsync();
        }

        public IPsoServer PsoServer
        {
            get
            {
                return this.GetValue<IPsoServer>(PsoServerProperty);
            }
            set
            {
                this.SetValue(PsoServerProperty, value);
            }
        }
        public static readonly PropertyData PsoServerProperty = RegisterProperty<PsoServerViewModel, IPsoServer>((x) => x.PsoServer);

        public int Port
        {
            get
            {
                return this.GetValue<int>(PortProperty);
            }
            set
            {
                this.SetValue(PortProperty, value);
            }
        }
        public static readonly PropertyData PortProperty = RegisterProperty<PsoServerViewModel, int>((x) => x.Port, 9200);

        public ClientType[] ClientTypes
        {
            get
            {
                return this.GetValue<ClientType[]>(ClientTypesProperty);
            }
            set
            {
                this.SetValue(ClientTypesProperty, value);
            }
        }
        public static readonly PropertyData ClientTypesProperty = RegisterProperty<PsoServerViewModel, ClientType[]>((x) => x.ClientTypes);

        public ClientType SelectedClientType
        {
            get
            {
                return this.GetValue<ClientType>(SelectedClientTypeProperty);
            }
            set
            {
                this.SetValue(SelectedClientTypeProperty, value);
            }
        }
        public static readonly PropertyData SelectedClientTypeProperty = RegisterProperty<PsoServerViewModel, ClientType>((x) => x.SelectedClientType, () => ClientType.Gamecube);

        public TaskCommand StartServerTaskCommand { get; private set; }
        
        private bool _CanExecuteStartServerTaskCommand()
        {
            return this.Port > 0;
        }

        private async Task _ExecuteStartServerTaskCommand()
        {
            var port = this.Port;
            var clientType = this.SelectedClientType;
            if (port > 0)
            {
                var client = await this.PsoServer.AwaitConnectionAsync(this.Port, System.Net.IPAddress.Any, clientType);
                //fire and forget
                var w = new PsoServerClientConnectionWindow(client);
                w.Show();
            }
        }

        public override string Title { get { return "View model title"; } }
    }
}
