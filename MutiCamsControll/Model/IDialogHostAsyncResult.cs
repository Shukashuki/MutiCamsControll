using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiCamsControl.Model
{
    public interface IDialogHostAsyncResult : IAsyncResult
    {
        bool DialogResult { get; }
        object Payload { get; }
    }
}
