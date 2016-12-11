namespace PsoPatchEditor.ViewModels
{
    using Catel.Data;
    using Catel.MVVM;
    using LibPSO.PsoPatcher;
    using System;
    using System.Threading.Tasks;

    public class RedirectViewModel : ViewModelBase
    {
        public RedirectViewModel(PsoRedirect redirect)
        {
            this.Redirect = redirect;
        }

        public override string Title { get { return "View model title"; } }

        // TODO: Register models with the vmpropmodel codesnippet
        // TODO: Register view model properties with the vmprop or vmpropviewmodeltomodel codesnippets
        // TODO: Register commands with the vmcommand or vmcommandwithcanexecute codesnippets

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            // TODO: subscribe to events here
        }

        protected override async Task CloseAsync()
        {
            // TODO: unsubscribe from events here

            await base.CloseAsync();
        }

        [Model]
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public PsoRedirect Redirect
        {
            get { return GetValue<PsoRedirect>(RedirectProperty); }
            set { SetValue(RedirectProperty, value); }
        }

        /// <summary>
        /// Register the Redirect property so it is known in the class.
        /// </summary>
        public static readonly PropertyData RedirectProperty = RegisterProperty("Redirect", typeof(PsoRedirect), null);


        [ViewModelToModel]
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        /// <summary>
        /// Register the Name property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string), null);

        [ViewModelToModel]
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string IPAddress
        {
            get { return GetValue<string>(IPAddressProperty); }
            set { SetValue(IPAddressProperty, value); }
        }

        /// <summary>
        /// Register the IPAddress property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IPAddressProperty = RegisterProperty("IPAddress", typeof(string), null);

        [ViewModelToModel]
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public UInt16 Port
        {
            get { return GetValue<UInt16>(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        /// <summary>
        /// Register the Port property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PortProperty = RegisterProperty("Port", typeof(UInt16), null);
    }
}
