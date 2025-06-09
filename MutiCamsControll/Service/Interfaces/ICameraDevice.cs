using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiCamsControl.Service.Interfaces
{
    public interface ICameraDevice : IDisposable
    {
        bool IsRunning {  get; }
        Task StartAsync();
        Task<Bitmap> CaptureAsync();    // 每次拍照都會延遲
        Task StopAsync();
        int CameraIndex { get; }
        string CameraName { get; }
        event Action<Bitmap> FrameCaptured;

        event Action CameraReady;

    }

    

    public interface ICameraManager
    {
        IReadOnlyList<ICameraDevice> Cameras { get; }        

        Task InitializeAllAsync();               // 啟動所有相機
        Task<IList<Bitmap>> CaptureAllAsync();   // 同步拍攝所有相機
        Task StopAllAsync();                     // 停止所有相機
        event Action CameraReady;
        event Action<int, Bitmap> FrameCaptured; // 每顆相機拍到圖時通知，帶上 index
    }



}
