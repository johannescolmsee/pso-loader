using Controls.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls.Services
{
    public class ClipboardService : IClipboardService
    {
        private Catel.Services.IDispatcherService _DispatcherService;
        public ClipboardService(Catel.Services.IDispatcherService dispatcherService)
        {
            this._DispatcherService = dispatcherService;
        }

        public Task SetTextAsync(string text)
        {
           return  this._DispatcherService.InvokeAsync(() => System.Windows.Clipboard.SetText(text));
        }
    }
}
