using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace MultiCamsControl.Model
{
    internal class ApiInteracter
    {
        public async Task<JObject> CallWebapiService(string filePath, string webServiceUrl)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                MessageBox.Show("未選擇檔案。", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            try
            {
                // 讀取檔案內容為二進位資料
                byte[] fileBytes = File.ReadAllBytes(filePath);

                using (HttpClient client = new HttpClient())
                using (var content = new MultipartFormDataContent())
                {
                    // 將檔案內容加入Request Body
                    ByteArrayContent fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "file",
                        FileName = Path.GetFileName(filePath)
                    };

                    // 將fileContent加入MultipartFormDataContent
                    content.Add(fileContent);

                    // 發送POST請求
                    HttpResponseMessage response = await client.PostAsync(webServiceUrl, content);

                    // 檢查回應是否成功
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"API 請求失敗: {response.StatusCode}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }

                    // 讀取回應內容
                    string responseContent = await response.Content.ReadAsStringAsync();
                    //MessageBox.Show($"API Response:{responseContent} ");

                    // 輸出回應結果到Console
                    Console.WriteLine($"HTTP 狀態碼: {response.StatusCode}");
                    Console.WriteLine($"回應內容: {responseContent}");

                    // 解析 JSON
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);

                    // 檢查是否存在 "inference_result" 屬性並返回結果
                    if (jsonResponse?.inference_result != null)
                    {
                        return jsonResponse; // 返回完整的 jsonResponse 給調用方
                    }
                    else
                    {
                        MessageBox.Show("回應中沒有 inference_result 屬性。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return null;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // 顯示 HTTP 請求例外訊息
                MessageBox.Show($"發生 HTTP 請求例外: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            catch (Exception ex)
            {
                // 顯示通用例外訊息
                MessageBox.Show($"發生例外: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<bool> TestUrl(string webServiceUrl)
        {
            if (string.IsNullOrWhiteSpace(webServiceUrl))
            {
                MessageBox.Show("未輸入URL。", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // 發送GET請求來測試URL是否能正常工作
                    HttpResponseMessage response = await client.GetAsync(webServiceUrl);

                    // 檢查是否成功
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"URL測試成功: {response.StatusCode}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        string responseContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"API Response:{responseContent} ");
                        return true;
                    }
                    else
                    {
                        MessageBox.Show($"URL測試成功");
                        return true;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"發生 HTTP 請求例外: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"發生例外: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
