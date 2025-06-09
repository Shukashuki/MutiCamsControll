using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MultiCamsControl.Service;

namespace MultiCamsControl.Service.Interfaces
{
    public interface IInferenceApiService
    {
        Task<YoloResult<YoloLabelResult>> DetectAsync(YoloImageInput input, CancellationToken ct = default);
        Task<YoloResult<bool>> PingAsync(CancellationToken ct = default);
    }
}
