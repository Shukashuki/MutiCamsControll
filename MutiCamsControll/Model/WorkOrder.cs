using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Xml;

namespace MultiCamsControl.Model
{



    public class WorkOrder : INotifyPropertyChanged
    {

        private bool found = false;
        private XmlNodeList TestItemNodeList;
        private string _shoporder = "";
        public string UserID = "";
        public string GPN = "";
        public string Qty = "0";
        public string Description = "";
        public int ProductIndex = 0;
        public string Product = "";
        public bool isEngineeringMode = false;
        private int _manualPassNum;
        private int _manualFailNum;


        public string Shoporder
        {
            get => _shoporder;
            set
            {
                Console.WriteLine($"Setting Shoporder. Old Value: {_shoporder}, New Value: {value}");

                if (_shoporder != value)
                {
                    _shoporder = value;
                    OnPropertyChanged(nameof(Shoporder));
                }
            }

        }
        // ManualPassNum 属性
        public int ManualPassNum
        {
            get => _manualPassNum;
            set
            {
                if (_manualPassNum != value)
                {
                    _manualPassNum = value;
                    Console.WriteLine("ManualPassNum has changed!"); // 確認這裡是否被調用
                    OnPropertyChanged(nameof(ManualPassNum));  // 這裡應該觸發事件
                }
            }
        }

        // ManualFailNum 属性
        public int ManualFailNum
        {
            get => _manualFailNum;
            set
            {
                if (_manualFailNum != value)
                {
                    _manualFailNum = value;
                    Console.WriteLine("ManualPassNum has changed!");
                    OnPropertyChanged(nameof(ManualFailNum));  // 当值改变时，触发 PropertyChanged 事件
                }
            }
        }
        public int AiPassNum { get; set; }
        public int AiFailNum { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            Console.WriteLine($"PropertyChanged triggered for {propertyName}");  // 添加一個檢查點
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /*private void UpdatePoductList(int defaultIndex = 3)
        {
            try
            {
                if (File.Exists(@"\\t1-pe-support3\PI_Project\Automation\Membrane_Vision\ConfigMappingTable\ProductList.ini"))
                {
                    File.Copy(@"\\t1-pe-support3\PI_Project\Automation\Membrane_Vision\ConfigMappingTable\ProductList.ini", @"C:\work\Membrane_Vision\Config\ProductList.ini", true);
                }
                else
                {
                    MessageBox.Show("下載產品清單異常或電腦使用者帳戶無權限\r\n請確認電腦使用者帳戶有權限開啟下列資料夾\\t1-pe-support3\\PI_Project\\Automation\\Membrane_Vision\\ConfigMappingTable\\ProductList.ini \r\n若無法開啟資料夾請確認網路連線或找MIS開通電腦使用者帳戶權限\r\n否則將使用Local模式生產", "網路異常", MessageBoxButton.OK);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("無法下載產品清單\r\n" + e.ToString(), "網路異常", MessageBoxButton.OK);
            }


            if (!File.Exists(@"C:\work\Membrane_Vision\Config\ProductList.ini"))
            {
                MessageBox.Show("偵測不到產品清單", "網路異常", MessageBoxButton.OK);
            }

            ZeIni ini = new ZeIni(@"C:\work\Membrane_Vision\Config\ProductList.ini");
            cbProduct.Items.Clear();
            for (int i = 0; i < Convert.ToInt32(ini.GetKeyValue("List", "Count", "0")); i++)
            {
                string s = ini.GetKeyValue("List", i.ToString(), "Empty");
                cbProduct.Items.Add(s);
                try
                {
                    if (File.Exists(ServerDeviceBackUpPath_Product + s + ".ini"))
                    {
                        File.Copy(ServerDeviceBackUpPath_Product + s + ".ini", Local_Product + s + ".ini", true);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("無法下載產品清單\r\n" + e.ToString(), "網路異常", MessageBoxButton.OK);
                }
            }
            ini.Dispose();
            cbProduct.SelectedIndex = defaultIndex;
        }*/

        /*public List<string> DownloadProgramList(string ProgramPath)
        {
            List<string> Rtn = new List<string>();
            try
            {
                //ZeIni ini = new ZeIni(@"C:\work\Membrane_Vision\Config\ProductList.ini");
                //cbProduct.Items.Clear();
                for (int i = 0; i < Convert.ToInt32(ini.GetKeyValue("List", "Count", "0")); i++)
                {
                   // cbProduct.Items.Add(ini.GetKeyValue("List", i.ToString(), "Empty"));
                    //Rtn.Add(ini.GetKeyValue("List", i.ToString(), "Empty"));
                }
                //ini.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("偵測不到產品清單\r\n" + ex.ToString(), "網路異常", MessageBoxButton.OK);
            }
            return Rtn;
        }*/

        public bool GetServiceResult(string Soid)
        {
            string serviceUrl = $"https://ws.garmin.com.tw/jobwebservice/generalservice.asmx/getJobInfoByJobNum?sJobName={Soid}&sOrg=";
            found = false;
            HttpWebRequest HttpWReq;
            HttpWebResponse HttpWResp;
            HttpWReq = (HttpWebRequest)WebRequest.Create(serviceUrl);
            HttpWReq.Method = "GET";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string webContent = reader.ReadToEnd();

                // 输出响应内容以检查
                // 检查是否是有效的XML内容

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(webContent);  // 如果不是有效的XML数据会抛出异常
                try
                {

                    doc.LoadXml(webContent);
                }
                catch (XmlException ex)
                {
                    MessageBox.Show("收到的响应不是有效的XML数据。请检查服务的返回内容。", "XML解析错误");
                    return true;

                }
            }



            try
            {
                HttpWResp = (HttpWebResponse)HttpWReq.GetResponse();
                if (HttpWResp.StatusCode == HttpStatusCode.OK)
                {
                    //Consume webservice with basic XML reading, assumes it returns (one) string
                    //convert stream 2 string      
                    StreamReader webreader = new StreamReader(HttpWResp.GetResponseStream());
                    string WebContent = webreader.ReadToEnd();

                    WebContent = WebContent.Replace("&lt;", "<");
                    WebContent = WebContent.Replace("&gt;", ">");
                    string ReplaceString = "<string xmlns=\"http://garmin.com.tw/\">";
                    int StartPos = WebContent.IndexOf(ReplaceString) + 38;
                    int EndPos = WebContent.IndexOf("</string>");
                    WebContent = WebContent.Substring(StartPos, EndPos - StartPos);

                    XmlDocument JobInfo = new XmlDocument();
                    JobInfo.LoadXml(WebContent);
                    XmlNodeList GTXFileList = JobInfo.DocumentElement.ChildNodes;

                    for (int i = 0; i < GTXFileList.Count; i++)
                    {
                        XmlNode CurrentPackageNode = GTXFileList.Item(i);

                        if (CurrentPackageNode.Name == "Header")
                        {
                            MessageBox.Show("此工單存在，請確認其他資訊無誤");

                            TestItemNodeList = CurrentPackageNode.ChildNodes;
                            Shoporder = Soid;
                            Qty = CurrentPackageNode.Attributes["QTY"].Value;
                            GPN = CurrentPackageNode.LastChild.Attributes["GPN"].Value;
                            Description = CurrentPackageNode.LastChild.Attributes["Description"].Value;
                            found = true;
                            break;
                        }

                        if (CurrentPackageNode.Name == "PACK_Job")
                        {
                            MessageBox.Show("此工單存在，請確認其他資訊無誤");
                            TestItemNodeList = CurrentPackageNode.ChildNodes;
                        }
                    }

                }
                else
                {
                    throw new Exception("Error on remote IP to Country service: " + HttpWResp.StatusCode.ToString());

                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return found;
        }

        public string MappingINI(string gpn, string defaultIni)
        {
            string rtn = defaultIni + ".ini";
            try
            {
                if (File.Exists(@"\\t1-pe-support3\PI_Project\Automation\Membrane_Vision\ConfigMappingTable\" + gpn.Substring(0, 9) + ".ini"))
                {
                    //ZeIni ini = new ZeIni(@"\\t1-pe-support3\PI_Project\Automation\Membrane_Vision\ConfigMappingTable\" + gpn.Substring(0, 9) + ".ini");
                    //rtn = ini.GetKeyValue(GPN.Substring(10, 2), "ConfigFile", "");
                    //ini.Dispose();
                }
                if (rtn == "")
                    rtn = defaultIni + ".ini";
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                MessageBox.Show(e.ToString());
            }
            return rtn;
        }






    }
}
