namespace PsoPatchEditor.ViewModels
{
    using Catel.Data;
    using Catel.MVVM;
    using LibPSO.PsoPatcher;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class XmlPatchDefinitionListItemViewModel : XmlPatchDefinitionViewModelBase
    {
        public XmlPatchDefinitionListItemViewModel(XmlPatchDefinition definition)
            : base(definition)
        {
        }

        public override string Title { get { return "XmlPatchDefinitionListItemViewModel"; } }

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

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string ValueString
        {
            get { return GetValue<string>(ValueStringProperty); }
            set { SetValue(ValueStringProperty, value); }
        }

        /// <summary>
        /// Register the ValueString property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ValueStringProperty = RegisterProperty("ValueString", typeof(string), null);

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == ByteValuesProperty.Name || e.PropertyName == StringValueProperty.Name || e.PropertyName == AddTerminatingZeroProperty.Name)
            {
                var bytes = !String.IsNullOrEmpty(this.StringValue) ?
                    this.StringValue.Select(x => (byte)x).Concat(this.AddTerminatingZero ? new byte[] { 0 } : new byte[0]) : this.ByteValues ?? new byte[0];
                this.ValueString = String.Join(" ", bytes.Select(y => String.Format(@"{0:x2}", y)));
            }
            base.OnPropertyChanged(e);
        }
    }
}
