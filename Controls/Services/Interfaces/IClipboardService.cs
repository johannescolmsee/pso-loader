using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controls.Services.Interfaces
{
    public interface IClipboardService
    {
        Task SetTextAsync(string text);
    }
}
