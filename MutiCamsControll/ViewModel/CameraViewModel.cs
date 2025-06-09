/*using Garmin;
using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Collections.ObjectModel;
using System.Drawing;
using MultiCamsControl.Model;
using System.Text.Json;

namespace MultiCamsControl
{
    public class CameraViewModel : TBaseViewModel
    {
        private readonly CameraModel _cameraModel;
        private readonly ImageProcessor _imageProcessor;
        private BitmapSource _cameraImage;
        private BitmapSource _snapImage;
        private ImageRecordModel _imageRecordModel;
        private string _currentImagePath;
        private string _recognitionResult;

        private readonly WorkOrder _workOrder = new WorkOrder();
        private string _shopOrder;
        private string _gpn;
        private string _qty;
        private string _description;
        private int _manualPassNum;
        private int _manualFailNum;

        // 新增状态变量
        private bool _isModelLoaded;
        private bool _isCameraOpened;
        private bool _isImageCaptured;        
        private string _snapImageClass;

        // 状态管理

        public Configuration config;

        public bool IsModelLoaded
        {
            get => _isModelLoaded;
            set
            {
                _isModelLoaded = value;
                OnPropertyChanged(nameof(IsModelLoaded));
                CommandManager.InvalidateRequerySuggested(); // 更新按钮状态
            }
        }

        public bool IsCameraOpened
        {
            get => _isCameraOpened;
            set
            {
                _isCameraOpened = value;
                OnPropertyChanged(nameof(IsCameraOpened));
                CommandManager.InvalidateRequerySuggested(); // 更新按钮状态
            }
        }



        public bool IsImageCaptured
        {
            get => _isImageCaptured;
            set
            {
                _isImageCaptured = value;
                OnPropertyChanged(nameof(IsImageCaptured));
                CommandManager.InvalidateRequerySuggested(); // 更新按钮状态
            }
        }
        public ObservableCollection<string> ClassificationResults => _cameraModel.ClassificationResults;
        public ObservableCollection<string> SnapClassificationResults => _cameraModel.SnapClassificationResults;

        public int ManualPassNum
        {
            get { return _manualPassNum; }
            set
            {
                _manualPassNum = value;
                OnPropertyChanged("Status");
            }
        }

        public int ManualFailNum
        {
            get { return _manualFailNum; ; }
            set
            {
                _manualFailNum = value;
                OnPropertyChanged("Status");
            }
        }

        public string SnapImageClass
        {
            get { return _snapImageClass; }
            set
            {
                _snapImageClass = value;
                OnPropertyChanged(nameof(SnapImageClass));
            }
        }
        public BitmapSource CameraImage
        {
            get { return _cameraImage; }
            set { RaiseAndSetIfChanged(ref _cameraImage, value); }
        }

        public BitmapSource SnapImage
        {
            get { return _snapImage; }
            set { RaiseAndSetIfChanged(ref _snapImage, value); }
        }

        

        // 命令来执行获取数据
        public ICommand FetchWorkOrderCommand { get; }

        // 命令
        public ICommand StartCameraCommand { get; }
        public ICommand StopCameraCommand { get; }
        public ICommand LoadModelCommand { get; }
        public ICommand LoadImageCommand { get; }
        public ICommand RunInferenceCommand { get; }
        public ICommand SnapCameraCommand { get; }
        public ICommand SavePassCommand { get; }
        public ICommand SaveFailCommand { get; }
        public ICommand ExportCsvCommand { get; }

        public CameraViewModel()
        {
            this.config = (Configuration)System.Windows.Application.Current.Resources["GlobalConfiguration"];
            string passFolder = @"C:\Automation\WatchClassify\log\Pass";
            string failFolder = @"C:\Automation\WatchClassify\log\Fail";
            string csvFilePath = @"C:\Automation\WatchClassify\log\ImageRecord.csv";

            if (!Directory.Exists(passFolder))
            {
                Directory.CreateDirectory(passFolder);
            }

            // 檢查並建立 Fail 資料夾
            if (!Directory.Exists(failFolder))
            {
                Directory.CreateDirectory(failFolder);
            }

            // 檢查並建立 log 資料夾 (用於存放 CSV 檔)
            string logFolder = System.IO.Path.GetDirectoryName(csvFilePath);
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            // 檢查 CSV 檔案是否存在，如果不存在可以選擇初始化它
            if (!File.Exists(csvFilePath))
            {
                using (var stream = File.Create(csvFilePath))
                {
                    // CSV 初始化處理，例如寫入標頭資訊
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.WriteLine("Time,ImageName,ClassificationResult,ManualResult");
                    }
                }
            }

            LoadConfig(System.Environment.MachineName);
            _cameraModel = new CameraModel(config.StationName.CameraName);
            _imageProcessor = new ImageProcessor();
            _cameraModel.FrameCaptured += OnFrameCaptured;
            SnapImageClass = "0";
            _imageRecordModel = new ImageRecordModel(passFolder, failFolder, csvFilePath);

            // 初始化命令
            SnapCameraCommand = new TBaseCommand(p => SnapCamera(), p => CanCaptureImage());
            StartCameraCommand = new TBaseCommand(p => StartCamera(), p => CanOpenCamera());
            StopCameraCommand = new TBaseCommand(p => StopCamera(), p => _isCameraOpened);
            LoadModelCommand = new TBaseCommand(p => LoadModel(), p => true);
            LoadImageCommand = new TBaseCommand(p => LoadImage(), p => CanOpenCamera());
            SavePassCommand = new TBaseCommand(p => SavePass(), p => CanSaveImage());
            SaveFailCommand = new TBaseCommand(p => SaveFail(), p => CanSaveImage());
            ExportCsvCommand = new TBaseCommand(p => ExportCsv(), p => true);
            
        }

        

        private void StartCamera()
        {
            _cameraModel.StartCameraAndInference();
            IsCameraOpened = true;
        }

        private void StopCamera()
        {
            _cameraModel.StopCamera();
            IsCameraOpened = false;
        }

        private void SnapCamera()
        {
            SnapImage = CameraImage;
            _cameraModel.UpdateSnapClassification();

            var bitmap = _imageProcessor.ProcessBitmapSourceImage(SnapImage);
                              
            var (predictedClass, probabilities) = _cameraModel.ProcessImageWithInference(bitmap);

            SnapImageClass = $"{predictedClass}";

            
            
            IsImageCaptured = true;
            
        }

        private void LoadImage()
        {
            StopCamera();
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                CameraImage = new BitmapImage(new Uri(openFileDialog.FileName));
                _cameraModel.Modelclassify(openFileDialog.FileName);
                
                IsCameraOpened = true;
            }
        }

        private Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memoryStream);
                memoryStream.Position = 0;

                Bitmap bitmap = new Bitmap(memoryStream);
                return bitmap;
            }
        }

        private void LoadModel()
        {
            using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath;
                    _cameraModel.LoadModel(folderPath);
                    IsModelLoaded = true;
                }
            }
        }

        private void OnFrameCaptured(BitmapSource bitmapSource)
        {
            CameraImage = bitmapSource;
        }

        public void SavePass()
        {
            if (_snapImage != null && SnapClassificationResults.Count > 0)
            {
                Bitmap snapImageBitmap = BitmapSourceToBitmap(_snapImage);
                _imageRecordModel.SaveImage(snapImageBitmap, _workOrder.Shoporder, SnapClassificationResults, "pass");
            }

            // 这里更新 ManualPassNum，同时触发 OnPropertyChanged
            ManualPassNum++;
            IsImageCaptured = false;

            // 更新 WorkOrder 中的 Pass 数量
            _workOrder.ManualPassNum++;
            OnPropertyChanged(nameof(ManualPassNum)); // 确保触发更新通知
        }

        public void SaveFail()
        {
            if (_snapImage != null && SnapClassificationResults.Count > 0)
            {
                Bitmap snapImageBitmap = BitmapSourceToBitmap(_snapImage);
                _imageRecordModel.SaveImage(snapImageBitmap, _workOrder.Shoporder, SnapClassificationResults, "fail");
            }

            // 这里更新 ManualFailNum，同时触发 OnPropertyChanged
            ManualFailNum++;
            IsImageCaptured = false;

            // 更新 WorkOrder 中的 Fail 数量
            _workOrder.ManualFailNum++;
            OnPropertyChanged(nameof(ManualFailNum)); // 确保触发更新通知
        }

        private void ExportCsv()
        {
            // 这里不需要做额外处理，CSV 每次保存时都会自动更新
        }

        // 控制按钮启用状态的逻辑

        private bool CanOpenCamera()
        {
            return IsModelLoaded; // 只有模型加载成功后才能打开相机
        }

        private bool CanCaptureImage()
        {
            return IsCameraOpened; // 只有相机成功打开后才能捕获图像
        }

        private bool CanSaveImage()
        {
            return IsImageCaptured; // 只有成功捕获图像后才能保存
        }

        ~CameraViewModel()
        {
            _cameraModel.Dispose();
        }

        public string CurrentImagePath
        {
            get => _currentImagePath;
            set
            {
                _currentImagePath = value;
                OnPropertyChanged(nameof(CurrentImagePath));
            }
        }

        public string RecognitionResult
        {
            get => _recognitionResult;
            set
            {
                _recognitionResult = value;
                OnPropertyChanged(nameof(RecognitionResult));
            }
        }

        public void LoadConfig(string filePath)
        {
            //string filePath = "T1-PE-JASON-NB.json"; // 請確保文件路徑正確

            try
            {
                // 讀取 JSON 文件內容
                string jsonString = File.ReadAllText(filePath);

                // 將 JSON 內容解析為 C# 對象
                config = JsonSerializer.Deserialize<Configuration>(jsonString);

                // 使用解析後的數據
                Console.WriteLine("Station: " + config.StationName.Station);
                // 這裡可以繼續訪問其他屬性...
            }
            catch (Exception ex)
            {
                Console.WriteLine("讀取 JSON 檔案時發生錯誤: " + ex.Message);
            }
        }
    
    }
}
*/