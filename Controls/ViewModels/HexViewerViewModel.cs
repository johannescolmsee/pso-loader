namespace Controls.ViewModels
{
    using Catel.Data;
    using Catel.MVVM;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using System.Linq;
    using System.Globalization;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Controls.Services.Interfaces;
    using Common;
    using Controls.Models;

    public class HexViewerViewModel : ViewModelBase
    {
        Catel.Services.ISaveFileService _SaveFileService;
        //Catel.Services.IOpenFileService _OpenFileService;
        IClipboardService _ClipboardService;
        public HexViewerViewModel(byte[] model, Catel.Services.ISaveFileService saveFileService, IClipboardService clipboardService)
        {
            this.Model = model;
            this._SaveFileService = saveFileService;
            //this._OpenFileService = openFileService;
            this._ClipboardService = clipboardService;
            this.SaveByteArrayTaskCommand = new TaskCommand(this._ExecuteSaveByteArray, this._CanExecuteSaveByteArray);
            this.CopyToClipboardTaskCommand = new TaskCommand(this._ExecuteCopyToClipboardTaskCommand, this._CanExecuteCopyToClipboardTaskCommand);
        }

        public override string Title { get { return "View model title"; } }

        // TODO: Register models with the vmpropmodel codesnippet
        // TODO: Register view model properties with the vmprop or vmpropviewmodeltomodel codesnippets
        // TODO: Register commands with the vmcommand or vmcommandwithcanexecute codesnippets

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await _UpdateHexViewerText();
            // TODO: subscribe to events here
        }

        private async Task _UpdateHexViewerText()
        {
            var textHeight = new FormattedText("kjK", CultureInfo.InstalledUICulture, System.Windows.FlowDirection.LeftToRight, HexViewerSettings.Instance.DisplayFont, HexViewerSettings.Instance.FontSize, new SolidColorBrush(Colors.Black)).Height;
            var numLines = Math.Ceiling(this.Model.Length / 16.0);
            var numLinesPerScreen = Math.Floor(this.TextAreaHeight / textHeight);
            this.LinesPerScreen = numLinesPerScreen;
            this.VerticalScrollMaximum = Math.Ceiling(numLines - numLinesPerScreen);

            var firstLine = Math.Ceiling(this.VerticalScroll);

            var offset = (int)(firstLine * 16);


            this.HexViewerText = await _GetHexViewerText(this.Model, offset, (int)Math.Ceiling(numLinesPerScreen));
        }

        private Task<string> _GetHexViewerText(byte[] p, int offset, int maxlines)
        {
            return Task.Factory.StartNew(() =>
                {
                    var text = HexStringFormater.GetHexText(p, offset, 16, maxlines);
                    return text;
                }
            );

        }

        protected override async Task CloseAsync()
        {
            // TODO: unsubscribe from events here

            await base.CloseAsync();
        }

        public byte[] Model
        {
            get
            {
                return this.GetValue<byte[]>(ModelProperty);
            }
            set
            {
                this.SetValue(ModelProperty, value);
            }
        }
        public static readonly PropertyData ModelProperty = RegisterProperty<HexViewerViewModel, byte[]>((x) => x.Model);

        public string HexViewerText
        {
            get
            {
                return this.GetValue<string>(HexViewerTextProperty);
            }
            set
            {
                this.SetValue(HexViewerTextProperty, value);
            }
        }
        public static readonly PropertyData HexViewerTextProperty = RegisterProperty<HexViewerViewModel, string>((x) => x.HexViewerText);

        public double VerticalScrollMaximum
        {
            get
            {
                return this.GetValue<double>(VerticalScrollMaximumProperty);
            }
            set
            {
                this.SetValue(VerticalScrollMaximumProperty, value);
            }
        }
        public static readonly PropertyData VerticalScrollMaximumProperty = RegisterProperty<HexViewerViewModel, double>((x) => x.VerticalScrollMaximum, () => 0.0, _OnViewRelatedPropertyChanged);

        public double LinesPerScreen
        {
            get
            {
                return this.GetValue<double>(LinesPerScreenProperty);
            }
            set
            {
                this.SetValue(LinesPerScreenProperty, value);
            }
        }
        public static readonly PropertyData LinesPerScreenProperty = RegisterProperty<HexViewerViewModel, double>((x) => x.LinesPerScreen, () => 0.0, _OnViewRelatedPropertyChanged);

        public double VerticalScroll
        {
            get
            {
                return this.GetValue<double>(VerticalScrollProperty);
            }
            set
            {
                this.SetValue(VerticalScrollProperty, value);
            }
        }
        public static readonly PropertyData VerticalScrollProperty = RegisterProperty<HexViewerViewModel, double>((x) => x.VerticalScroll, () => 0.0, _OnViewRelatedPropertyChanged);


        public double TextAreaHeight
        {
            get
            {
                return this.GetValue<double>(TextAreaHeightProperty);
            }
            set
            {
                this.SetValue(TextAreaHeightProperty, value);
            }
        }
        public static readonly PropertyData TextAreaHeightProperty = RegisterProperty<HexViewerViewModel, double>((x) => x.TextAreaHeight, () => 0.0, _OnViewRelatedPropertyChanged);

        private static void _OnViewRelatedPropertyChanged(HexViewerViewModel arg1, AdvancedPropertyChangedEventArgs arg2)
        {
            arg1._OnViewRelatedPropertyChanged(arg2);
        }

        private void _OnViewRelatedPropertyChanged(AdvancedPropertyChangedEventArgs arg2)
        {
            var t = this._UpdateHexViewerText();
        }

        public TaskCommand SaveByteArrayTaskCommand { get; private set; }

        private bool _CanExecuteSaveByteArray()
        {
            return
                this.Model != null;
        }

        private Task _ExecuteSaveByteArray()
        {
            return _TrySave(this.Model);
        }

        private async Task _TrySave(byte[] p)
        {

            var ofs = this._SaveFileService;
            if (ofs.DetermineFile())
            {
                using (var fs = System.IO.File.Open(ofs.FileName, System.IO.FileMode.Create))
                {
                    await fs.WriteAsync(p, 0, p.Length);
                }
            }
        }

        public TaskCommand CopyToClipboardTaskCommand { get; private set; }
        
        private bool _CanExecuteCopyToClipboardTaskCommand()
        {
            return
                this.HexViewerText != null;
        }

        private async Task _ExecuteCopyToClipboardTaskCommand()
        {
            var bytes = this.Model;
            if (bytes != null)
            {
                var text = await this._GetHexViewerText(this.Model, 0, 0);
                await this._ClipboardService.SetTextAsync(text ?? "");
            }
        }
    }
}
