using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiCamsControl.Service;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media.Media3D;
using CommunityToolkit.Mvvm.Messaging;
using MultiCamsControl.message;
using MultiCamsControl.Service.Interfaces;

namespace MultiCamsControl.ViewModel
{
    public class MultiCamsViewModel : ObservableObject
    {
        private readonly ICameraManager _cameraManager;

        private int _readyCount = 0;

        private bool _isAllCamerasReady;
        public bool IsAllCamerasReady
        {
            get => _isAllCamerasReady;
            set => SetProperty(ref _isAllCamerasReady, value);
        }


        public ObservableCollection<Bitmap> Images { get; } = new ObservableCollection<Bitmap>();

        public IAsyncRelayCommand CaptureAllCommand { get; }

        public IAsyncRelayCommand StartAllCommand { get; }
        public MultiCamsViewModel(ICameraManager cameraManager)
        {
            _cameraManager = cameraManager;
            _cameraManager.FrameCaptured += OnFrameCaptured;
            foreach (var camera in _cameraManager.Cameras)
            {
                camera.CameraReady += OnSingleCameraReady;
            }

            CaptureAllCommand = new AsyncRelayCommand(CaptureAllAsync);
            StartAllCommand = new AsyncRelayCommand(StartAllCameras);
        }

        private void OnFrameCaptured(int index, Bitmap bmp)
        {
            while (Images.Count <= index)
                Images.Add(null);

            Images[index] = bmp;
        }

        public async Task CaptureAllAsync()
        {
            await _cameraManager.CaptureAllAsync();

            WeakReferenceMessenger.Default.Send(
            new ImagesCapturedMessage(Images.ToList()));
        }
        public async Task StartAllCameras()
        {
            await _cameraManager.InitializeAllAsync();
        }
        private void OnSingleCameraReady()
        {
            _readyCount++;

            if (_readyCount == _cameraManager.Cameras.Count)
            {
                IsAllCamerasReady = true;
            }
        }

    }
}
