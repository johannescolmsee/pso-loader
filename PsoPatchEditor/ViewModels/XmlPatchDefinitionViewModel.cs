namespace PsoPatchEditor.ViewModels
{
    using Catel.Data;
    using Catel.MVVM;
    using LibPSO.PsoPatcher;
    using System;
    using System.Threading.Tasks;

    public class XmlPatchDefinitionViewModel : XmlPatchDefinitionViewModelBase
    {
        public XmlPatchDefinitionViewModel(XmlPatchDefinition definition)
            : base(definition)
        {
        }

        public override string Title { get { return "XmlPatchDefinitionViewModel"; } }

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

        
    }
}
