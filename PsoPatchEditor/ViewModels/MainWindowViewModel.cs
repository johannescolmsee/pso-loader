namespace PsoPatchEditor.ViewModels
{
    using Catel.Data;
    using Catel.MVVM;
    using Common;
    using LibPSO;
    using LibPSO.PsoPatcher;
    using LibPSO.PsoVersionDetector;
    using PsoPatchEditor.Models.OldFormat;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class MainWindowViewModel : ViewModelBase
    {
        Catel.Services.IOpenFileService _OpenFileService;
        Catel.Services.ISaveFileService _SaveFileService;
        Catel.Services.IMessageService _MessageService;
        public MainWindowViewModel(Catel.Services.IOpenFileService openFileService, Catel.Services.ISaveFileService saveFileService, Catel.Services.IMessageService messageService)
        {
            this._OpenFileService = openFileService;
            this._SaveFileService = saveFileService;
            this._MessageService = messageService;
            this.LoadPatchOldFormatCommand = new TaskCommand(this._ExecuteLoadPatchOldFormatCommand, this._CanExecuteLoadPatchOldFormatCommand);
            this.SavePatchCommand = new TaskCommand(this._ExecuteSavePatchCommand, this._CanExecuteSavePatchCommand);
            this.LoadPatchCommand = new TaskCommand(this._ExecuteLoadPatchCommand, this._CanExecuteLoadPatchCommand);
            this.ExportBinaryCommand = new TaskCommand(this._ExecuteExportBinaryCommand, this._CanExecuteExportBinaryCommand);
            this.ExportPacketCommand = new TaskCommand(this._ExecuteExportPacketCommand, this._CanExecuteExportPacketCommand);
            this.NewPatchCommand = new TaskCommand(this._ExecuteNewPatchCommand, this._CanExecuteNewPatchCommand);
            this.AddPatchCommand = new TaskCommand(this._ExecuteAddPatchCommand, this._CanExecuteAddPatchCommand);
            this.AddRedirectCommand = new TaskCommand(this._ExecuteAddRedirectCommand, this._CanExecuteAddRedirectCommand);
            this.ConvertVersionCheckToBinaryCommand = new TaskCommand(this._ExecuteConvertVersionCheckToBinaryCommand, this._CanExecuteConvertVersionCheckToBinaryCommand);
        }

        public override string Title { get { return "PsoPatchEditor"; } }

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

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [Model]
        public PsoPatchDefinition PatchDefinition
        {
            get { return GetValue<PsoPatchDefinition>(PatchDefinitionProperty); }
            set { SetValue(PatchDefinitionProperty, value); }
        }

        [ViewModelToModel]
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

        /// <summary>
        /// Register the PatchDefinition property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PatchDefinitionProperty = RegisterProperty("PatchDefinition", typeof(PsoPatchDefinition), null);

        /// <summary>
        /// Gets the LoadPatchOldFormatCommand command.
        /// </summary>
        public TaskCommand LoadPatchOldFormatCommand { get; private set; }

        /// <summary>
        /// Method to check whether the LoadPatchOldFormatCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool _CanExecuteLoadPatchOldFormatCommand()
        {
            return true;
        }

        /// <summary>
        /// Method to invoke when the LoadPatchOldFormatCommand command is executed.
        /// </summary>
        private async Task _ExecuteLoadPatchOldFormatCommand()
        {
            Exception exception = null;
            try
            {
                if (this._OpenFileService.DetermineFile())
                {
                    var filename = this._OpenFileService.FileName;
                    var oldFormat = PsoPatchDefinitionOld.FromXml(filename);

                    this.PatchDefinition = oldFormat.GetPsoPatchDefinition();
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

        public TaskCommand SavePatchCommand { get; set; }

        private bool _CanExecuteSavePatchCommand()
        {
            return this.PatchDefinition != null;
        }

        private async Task _ExecuteSavePatchCommand()
        {
            Exception exception = null;
            try
            {
                var patchDef = this.PatchDefinition;
                if (patchDef != null && this._SaveFileService.DetermineFile())
                {
                    var filename = this._SaveFileService.FileName;

                    patchDef.ToXml(filename);
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

        public TaskCommand LoadPatchCommand { get; set; }

        private bool _CanExecuteLoadPatchCommand()
        {
            return true;
        }

        private async Task _ExecuteLoadPatchCommand()
        {
            Exception exception = null;
            try
            {
                if (this._OpenFileService.DetermineFile())
                {
                    var filename = this._OpenFileService.FileName;

                    this.PatchDefinition = PsoPatchDefinition.FromXml(filename);
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

        public TaskCommand ExportBinaryCommand { get; set; }

        private bool _CanExecuteExportBinaryCommand()
        {
            return this.PatchDefinition != null;
        }

        private async Task _ExecuteExportBinaryCommand()
        {
            Exception exception = null;
            try
            {
                var patchDef = this.PatchDefinition;
                if (patchDef != null && this._SaveFileService.DetermineFile())
                {
                    var filename = this._SaveFileService.FileName;
                    File.WriteAllBytes(filename, patchDef.GetPatchProgram());
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

        public TaskCommand ExportPacketCommand { get; set; }

        private bool _CanExecuteExportPacketCommand()
        {
            return this.PatchDefinition != null;
        }

        private async Task _ExecuteExportPacketCommand()
        {
            Exception exception = null;
            try
            {
                var patchDef = this.PatchDefinition;
                if (patchDef != null && this._SaveFileService.DetermineFile())
                {
                    var filename = this._SaveFileService.FileName;
                    File.WriteAllBytes(filename, Packets.GetUpdateCodePacket(patchDef.GetPatchProgram(), LibPSO.PsoServices.Interfaces.ClientType.Gamecube));
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

        public TaskCommand NewPatchCommand { get; set; }

        private bool _CanExecuteNewPatchCommand()
        {
            return true;
        }

        private async Task _ExecuteNewPatchCommand()
        {
            Exception exception = null;
            try
            {
                this.PatchDefinition = new PsoPatchDefinition();
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

        public TaskCommand AddPatchCommand { get; set; }

        private bool _CanExecuteAddPatchCommand()
        {
            return this.PatchDefinition != null;
        }

        private async Task _ExecuteAddPatchCommand()
        {
            Exception exception = null;
            try
            {
                var patchDef = this.PatchDefinition;
                if (patchDef != null)
                {
                    patchDef.Patches.Add(new XmlPatchDefinition());
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

        public TaskCommand AddRedirectCommand { get; set; }

        private bool _CanExecuteAddRedirectCommand()
        {
            return this.PatchDefinition != null && this.PatchDefinition.Redirect == null;
        }

        private async Task _ExecuteAddRedirectCommand()
        {
            Exception exception = null;
            try
            {
                var patchDef = this.PatchDefinition;
                if (patchDef != null)
                {
                    patchDef.Redirect = new PsoRedirect();
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

        public TaskCommand ConvertVersionCheckToBinaryCommand { get; set; }

        private bool _CanExecuteConvertVersionCheckToBinaryCommand()
        {
            return true;
        }

        private async Task _ExecuteConvertVersionCheckToBinaryCommand()
        {
            Exception exception = null;
            try
            {
                if (this._OpenFileService.DetermineFile())
                {
                    var filename = this._OpenFileService.FileName;

                    var versionDetect = PsoVersionDetectionDefinition.FromXml(filename);
                    if (this._SaveFileService.DetermineFile())
                    {
                        var filenameSave = this._SaveFileService.FileName;
                        File.WriteAllBytes(filenameSave, versionDetect.GetPsoVersionDetectionProgram());
                    }
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
    }
}
