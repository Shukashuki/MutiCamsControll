using DirectShowLib;
using MultiCamsControl.Service.Interfaces;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static MultiCameraManager;
using static MultiCamsControl.Service.CameraManagerProvider;


namespace MultiCamsControl.Service
{
    public class CameraManagerProvider : ICameraManagerProvider
    {
        private readonly Dictionary<string, ICameraManager> _managers;
        public ICameraManager Current { get; private set; }

        public IEnumerable<string> AvailableGroups => _managers.Keys;




        public void SwitchTo(string key)
        {
            if (_managers.ContainsKey(key))
                Current = _managers[key];
        }

        public CameraManagerProvider()
        {
            Dictionary<int, string> availableCameras = FindAvailableCamerasWithNames();
            foreach (var cam in availableCameras)
            {
                Debug.WriteLine($"[進階方法] 找到相機 - 索引: {cam.Key}, 名稱: {cam.Value}");
            }

            


            _managers = new Dictionary<string, ICameraManager>
        {
            { "Main", new MultiCameraManager(new List<ICameraDevice>
                {
                    new OpenCvCameraDevice(0),
                    new OpenCvCameraDevice(1),
                    new FakeCameraDevice(@"E:\Figure_1.png")
                })
            },
            { "Backup", new MultiCameraManager(new List<ICameraDevice>
                {
                    new FakeCameraDevice(@"E:\Figure_1.png"),
                    new FakeCameraDevice(@"E:\Figure_1.png"),
                    new FakeCameraDevice(@"E:\Figure_1.png")
                })
            }

        };
            Current = _managers["Main"];
        }

        #region 方法一: 簡易掃描相機索引
        /// <summary>
        /// 透過嘗試開啟來掃描並返回所有可用的相機索引。
        /// </summary>
        /// <param name="maxCheckCount">最大檢查索引數量</param>
        /// <returns>可用的相機索引列表</returns>
        public List<int> FindAvailableCameraIndices(int maxCheckCount = 10)
        {
            var availableIndices = new List<int>();
            for (int i = 0; i < maxCheckCount; i++)
            {
                // 嘗試建立 VideoCapture 物件來檢查相機是否存在
                using (var capture = new VideoCapture(i))
                {
                    // 如果 IsOpened() 為 true，表示此索引對應的相機可用
                    if (capture.IsOpened())
                    {
                        availableIndices.Add(i);
                    }
                }
            }
            return availableIndices;
        }
        #endregion

        #region 方法二: 使用 DirectShow 取得相機列表 (推薦)
        /// <summary>
        /// 使用 DirectShowLib 取得所有可用的視訊輸入設備（相機）
        /// </summary>
        /// <returns>一個包含相機索引和名稱的字典</returns>
        public Dictionary<int, string> FindAvailableCamerasWithNames()
        {
            var cameras = new Dictionary<int, string>();
            // 找出所有視訊輸入設備
            var videoInputDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            for (int i = 0; i < videoInputDevices.Length; i++)
            {
                // 嘗試使用 OpenCvSharp 確認此索引是否真的可以開啟
                using (var capture = new VideoCapture(i))
                {
                    if (capture.IsOpened())
                    {
                        cameras.Add(i, videoInputDevices[i].Name);
                    }
                }
            }
            return cameras;
        }
        #endregion
    }

    public static class CameraFactory
    {
        public static List<ICameraDevice> CreateDevices(params int[] indices)
        {
            return indices.Select(i => new OpenCvCameraDevice(i)).Cast<ICameraDevice>().ToList();
        }
    }
}


