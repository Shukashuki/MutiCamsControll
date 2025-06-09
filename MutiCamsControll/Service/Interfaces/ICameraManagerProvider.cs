using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiCamsControl.Service.Interfaces
{
    public interface ICameraManagerProvider
    {
        ICameraManager Current { get; }
        void SwitchTo(string key);
        IEnumerable<string> AvailableGroups { get; }
    }
}
