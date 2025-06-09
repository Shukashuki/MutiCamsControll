using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MultiCamsControl.Service;
using MultiCamsControl.Service.Interfaces;

namespace MultiCamsControl.ViewModel
{

    /// <summary>
    /// 聚合 MultiCams 與 Detection 的 Facade ViewModel。
    /// </summary>
    public partial class Func1ViewModel : ObservableObject
    {
        // 允許 View（或其他 VM）讀取各子 VM
        private MultiCamsViewModel _cams;
        public MultiCamsViewModel Cams
        {
            get => _cams;
            set
            {
                if (_cams != value)
                {
                    _cams = value;
                    OnPropertyChanged(nameof(Cams));
                }
            }
        }

        public DetectionViewModel Detector { get; }

        private readonly ICameraManagerProvider _provider;
        public ObservableCollection<string> CameraGroups => new(_provider.AvailableGroups);

        private string _selectedCameraGroup;
        public string SelectedCameraGroup
        {
            get => _selectedCameraGroup;
            set
            {
                if (_selectedCameraGroup != value)
                {
                    _selectedCameraGroup = value;
                    OnPropertyChanged(nameof(SelectedCameraGroup));
                }
            }
        }


        public ObservableCollection<ImageResultPair> Results { get; } =
            new ObservableCollection<ImageResultPair>
            {
                new(), new(), new()          // 先佔 3 格
            };

        public BitmapSource? ImageSource => ConvertBitmapToSource(Cams.Images.FirstOrDefault());

        // 將偵測結果直接 expose 給 View 做 ItemsControl/ListView 綁定
        public ObservableCollection<RecordResult> Predictions => Detector.Predictions;

        public ObservableCollection<Bitmap> RawImages => Cams.Images;
        public ObservableCollection<RecordResult> RawPredictions => Detector.Predictions;




        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string? _status;
        public string? Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public IAsyncRelayCommand StartWorkflowCommand { get; }
        public IRelayCommand SwitchCameraGroupCommand { get; }

        public Func1ViewModel(ICameraManagerProvider provider, DetectionViewModel detector)
        {
            _provider = provider;
            Detector = detector;

            // 初始化 3 組資料結構
            Results = new ObservableCollection<ImageResultPair>
                {
                    new(), new(), new()
                };

            StartWorkflowCommand = new AsyncRelayCommand(StartWorkflowAsync);

            Cams = new MultiCamsViewModel(_provider.Current);

            SwitchCameraGroupCommand = new RelayCommand(SwitchCameraGroup);
        }


        /// <summary>
        /// 一鍵流程：啟動所有相機 → 擷取影像 → 執行推論。
        /// </summary>
        private async Task StartWorkflowAsync()
        {
            try
            {
                IsBusy = true;
                Status = "Initializing cameras…";

                await Cams.StartAllCameras();

                if (!Cams.IsAllCamerasReady)
                {
                    Status = "Camera init failed.";
                    return;
                }

                Status = "Capturing...";
                await Cams.CaptureAllAsync();

                if (Cams.Images.Count < 3)
                {
                    Status = "Not enough images captured.";
                    return;
                }

                for (int i = 0; i < 3; i++)
                {
                    var bmp = Cams.Images[i];
                    Results[i].Image = ConvertBitmapToSource(bmp);

                    var input = new YoloImageInput(bmp, "SO001", new[] { "class1" });
                    var detectR = await Detector.RunInferenceAsync(input);

                    Results[i].Predictions.Clear();
                    if (detectR.IsSuccess && detectR.Data != null)
                    {
                        foreach (var p in detectR.Data.Predictions)
                            Results[i].Predictions.Add(p);
                    }
                }

                Status = "Inference completed.";
            }
            finally
            {
                IsBusy = false;
                StartWorkflowCommand.NotifyCanExecuteChanged();
            }
        }

        private void SwitchCameraGroup()
        {
            _provider.SwitchTo(SelectedCameraGroup);

            // 替換相機管理 VM
            Cams = new MultiCamsViewModel(_provider.Current);

            // 清空或重建對應的結果
            Results.Clear();
            for (int i = 0; i < 3; i++)
                Results.Add(new ImageResultPair());

            Status = $"已切換至相機群組：{SelectedCameraGroup}";
        }


        private BitmapSource? ConvertBitmapToSource(Bitmap? bitmap)
        {
            if (bitmap == null) return null;

            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = ms;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze(); // 重要：避免跨執行緒例外

            return bmp;
        }


    }

    public partial class ImageResultPair : ObservableObject
    {
        private BitmapSource? _image;
        public BitmapSource? Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        public ObservableCollection<RecordResult> Predictions { get; } =
            new ObservableCollection<RecordResult>();
    }

}
