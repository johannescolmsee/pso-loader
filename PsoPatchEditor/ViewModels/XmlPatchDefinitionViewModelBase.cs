namespace PsoPatchEditor.ViewModels
{
    using Catel.Data;
    using Catel.MVVM;
    using LibPSO.PsoPatcher;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class XmlPatchDefinitionViewModelBase : ViewModelBase
    {
        public XmlPatchDefinitionViewModelBase(XmlPatchDefinition definition)
        {
            this.Model = definition;
        }

        public override string Title { get { return "XmlPatchDefinitionViewModelBase"; } }

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
        [Model]
        public XmlPatchDefinition Model
        {
            get { return GetValue<XmlPatchDefinition>(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        /// <summary>
        /// Register the Model property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ModelProperty = RegisterProperty("Model", typeof(XmlPatchDefinition), null);


        [ViewModelToModel()]
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

        [ViewModelToModel()]
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public uint Address
        {
            get { return GetValue<uint>(AddressProperty); }
            set { SetValue(AddressProperty, value); }
        }

        /// <summary>
        /// Register the Address property so it is known in the class.
        /// </summary>
        public static readonly PropertyData AddressProperty = RegisterProperty("Address", typeof(uint), 0);


        [ViewModelToModel()]
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>        
        public string StringValue
        {
            get { return GetValue<string>(StringValueProperty); }
            set { SetValue(StringValueProperty, value); }
        }

        /// <summary>
        /// Register the StringValue property so it is known in the class.
        /// </summary>
        public static readonly PropertyData StringValueProperty = RegisterProperty("StringValue", typeof(string), null);

        [ViewModelToModel()]
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool AddTerminatingZero
        {
            get { return GetValue<bool>(AddTerminatingZeroProperty); }
            set { SetValue(AddTerminatingZeroProperty, value); }
        }

        /// <summary>
        /// Register the AddTerminatingZero property so it is known in the class.
        /// </summary>
        public static readonly PropertyData AddTerminatingZeroProperty = RegisterProperty("AddTerminatingZero", typeof(bool), false);

        [ViewModelToModel()]
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public byte[] ByteValues
        {
            get { return GetValue<byte[]>(ByteValuesProperty); }
            set { SetValue(ByteValuesProperty, value); }
        }

        /// <summary>
        /// Register the ByteValues property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ByteValuesProperty = RegisterProperty("ByteValues", typeof(byte[]), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool HasErrorsOrWarnings
        {
            get { return GetValue<bool>(HasErrorsOrWarningsProperty); }
            private set { SetValue(HasErrorsOrWarningsProperty, value); }
        }

        /// <summary>
        /// Register the HasErrorsOrWarnings property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HasErrorsOrWarningsProperty = RegisterProperty("HasErrorsOrWarnings", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string ErrorsAndWarningText
        {
            get { return GetValue<string>(ErrorsAndWarningTextProperty); }
            private set { SetValue(ErrorsAndWarningTextProperty, value); }
        }

        /// <summary>
        /// Register the ErrorsAndWarningText property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ErrorsAndWarningTextProperty = RegisterProperty("ErrorsAndWarningText", typeof(string), null);

        protected override void ValidateFields(System.Collections.Generic.List<IFieldValidationResult> validationResults)
        {
            if ((this.ByteValues == null || this.ByteValues.Length == 0) && String.IsNullOrEmpty(this.StringValue))
            {
                validationResults.Add(FieldValidationResult.CreateWarning(() => this.ByteValues, "No values set."));
            }
            if (this.ByteValues != null && this.ByteValues.Length > 0 && !String.IsNullOrEmpty(this.StringValue))
            {
                validationResults.Add(FieldValidationResult.CreateWarning(() => this.ByteValues, "StringValue will override ByteValues."));
            }
            if (this.Address < 0x80000000
                ||
                (this.Address >= 0x81800000 && this.Address < 0xC0000000)
                ||
                this.Address >= 0xC1800000
                )
            {
                validationResults.Add(FieldValidationResult.CreateWarning(() => this.ByteValues, "Address is out of range."));
            }
            base.ValidateFields(validationResults);

            this.HasErrorsOrWarnings = validationResults.Any();
            var messages = validationResults.Select(x => x.Message);
            this.ErrorsAndWarningText = String.Join(Environment.NewLine, messages.Where(x => !String.IsNullOrEmpty(x)));
        }
    }
}
