using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using MultiCamsControl.Model;
using MultiCamsControl.Service.Interfaces;


public class OpenCvCameraDevice : ICameraDevice
    {
        private VideoCapture _capture;
        private bool _isRunning = false;
        private readonly int _cameraIndex;
        private readonly string _cameraName;
        public bool IsRunning => _isRunning;
        public int CameraIndex => _cameraIndex;
        public string CameraName => _cameraName;
        public event Action CameraReady;
        public event Action<Bitmap> FrameCaptured;

        public OpenCvCameraDevice(int cameraIndex)
        {
            _cameraIndex = cameraIndex;
        }

    public async Task StartAsync()
    {
        if (_isRunning) return;

        await Task.Run(() =>
        {
            _capture = new VideoCapture(_cameraIndex, VideoCaptureAPIs.DSHOW);
        });

        if (!_capture.IsOpened())
        {
            _isRunning = false;
            throw new InvalidOperationException($"Camera {_cameraIndex} failed to open.");
        }

        _isRunning = true;
        CameraReady?.Invoke();

        // 模擬即時影像流（選用）
        _ = Task.Run(async () =>
        {
            using var mat = new Mat();
            while (_isRunning)
            {
                _capture.Read(mat);
                if (_capture == null || !_capture.IsOpened())
                    throw new InvalidOperationException("Camera not initialized.");
                if (!mat.Empty())
                {
                    var bmp = BitmapConverter.ToBitmap(mat);
                    FrameCaptured?.Invoke(bmp);
                }
                await Task.Delay(100); // 每 100ms 擷取一次
            }
        });
    }

    public async Task<Bitmap> CaptureAsync()
    {
        if (_capture == null || !_capture.IsOpened())
            throw new InvalidOperationException("Capture failed: camera not started.");

        using (var mat = new Mat())
        {
            await Task.Run(() =>
            {
                _capture.Read(mat);
            });

            if (!mat.Empty())
            {
                return BitmapConverter.ToBitmap(mat);
            }
            else
            {
                throw new InvalidOperationException("Capture failed: empty frame.");
            }
        }
    }

    public Task StopAsync()
        {
            if (_isRunning)
            {
                _capture?.Release();
                _isRunning = false;
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            StopAsync().Wait(); 
            _capture?.Dispose();
        }
    }


public class MultiCameraManager : ICameraManager
{
    private readonly List<ICameraDevice> _cameras;

    

    public IReadOnlyList<ICameraDevice> Cameras => _cameras;
    public event Action<int, Bitmap> FrameCaptured;
    public event Action CameraReady;

    public MultiCameraManager(IEnumerable<ICameraDevice> cameras)
    {
        _cameras = cameras.ToList();

        for (int i = 0; i < _cameras.Count; i++)
        {
            int index = i;
            _cameras[i].FrameCaptured += bmp =>
            {
                FrameCaptured?.Invoke(index, bmp);
            };
        }
    }

    public async Task InitializeAllAsync()
    {

        await StopAllAsync();
        await Task.WhenAll(_cameras.Select(cam => cam.StartAsync()));
    }

    public async Task<IList<Bitmap>> CaptureAllAsync()
    {
        var captureTasks = _cameras.Select(cam => cam.CaptureAsync());
        return (await Task.WhenAll(captureTasks)).ToList();
    }

    public async Task StopAllAsync()
    {
        await Task.WhenAll(_cameras.Select(cam => cam.StopAsync()));
    }

    public void Dispose()
    {
        foreach (var camera in _cameras)
        {
            camera.Dispose();
        }
    }

    public class FakeCameraDevice : ICameraDevice
    {
        private readonly string _testImagePath;

        public FakeCameraDevice(string testImagePath)
        {
            _testImagePath = testImagePath;
        }

        public bool IsRunning { get; private set; } = false;
        public int CameraIndex => -1;
        public string CameraName => "FakeCamera";

        public event Action CameraReady;
        public event Action<Bitmap> FrameCaptured;

        public Task StartAsync()
        {
            IsRunning = true;
            CameraReady?.Invoke();
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            IsRunning = false;
            return Task.CompletedTask;
        }

        public Task<Bitmap> CaptureAsync()
        {
            // 從檔案載入圖片
            var bmp = new Bitmap(_testImagePath);
            FrameCaptured?.Invoke(bmp);
            return Task.FromResult(bmp);
        }

        public void Dispose() => StopAsync().Wait();
    }




}

