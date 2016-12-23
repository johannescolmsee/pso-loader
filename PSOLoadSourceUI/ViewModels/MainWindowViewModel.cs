namespace PSOLoadSourceUI.ViewModels
{
    using Catel.MVVM;
    using System.Threading.Tasks;

    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
        }

        public override string Title { get { return "PSOLoadSourceUI"; } }

        // TODO: Register models with the vmpropmodel codesnippet
        // TODO: Register view model properties with the vmprop or vmpropviewmodeltomodel codesnippets
        // TODO: Register commands with the vmcommand or vmcommandwithcanexecute codesnippets

        protected override Task InitializeAsync()
        {
            return base.InitializeAsync();

            // TODO: subscribe to events here
        }

        protected override Task CloseAsync()
        {
            // TODO: unsubscribe from events here

            return base.CloseAsync();
        }

        private byte[] _Bytes = new byte[1024];
        public byte[] Bytes
        {
            get
            {
                return _Bytes;
            }
        }
    }
}
