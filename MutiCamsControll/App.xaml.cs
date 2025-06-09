using MultiCamsControl.Service;
using MultiCamsControl.Service.Interfaces;
using MultiCamsControl.ViewModel;
using System.Windows;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
// using static MultiCameraManager; // <-- 暫時註解，直到您提供了這個類別

namespace MultiCamsControl
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider? Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                App.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

                // === DI 註冊 ===
                var services = new ServiceCollection();
                ConfigureServices(services);
                Services = services.BuildServiceProvider();

                // === 啟動主視窗 (這是您缺少的關鍵部分) ===
                var mainWindow = Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"應用程式啟動失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // 註冊使用 HttpClient 的服務
            services.AddSingleton<IInferenceApiService>(sp =>
            {
                var httpClient = new HttpClient
                {
                    // 建議將此位址移至設定檔 (appsettings.json)
                    BaseAddress = new Uri("http://linxpa-dl02.garmin.com:5316")
                };
                return new YoloDetectionService("/inference", httpClient);
            });

            // 註冊儲存用服務
            services.AddSingleton<IImageRecordService>(sp =>
            {
                // 建議將這些路徑移至設定檔
                var passPath = @"C:\Automation\WatchClassify\Pass";
                var failPath = @"C:\Automation\WatchClassify\Fail";
                return new ImageRecordService(passPath, failPath);
            });

            // 註冊 Camera 管理服務 (修正重複註冊)
            services.AddSingleton<ICameraManagerProvider, CameraManagerProvider>();
            services.AddSingleton<IInferenceApiService, FakeInferenceService>();

            // 註冊 ViewModels
            services.AddSingleton<DetectionViewModel>();
            services.AddSingleton<Func1ViewModel>();

            // 註冊主視窗
            services.AddSingleton<MainWindow>();
        }
    }
}