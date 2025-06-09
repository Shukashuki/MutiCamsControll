using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MultiCamsControl.Service.Interfaces;
using MultiCamsControl.Service;

namespace MultiCamsControl.Service
{
    public class YoloDetectionService : IInferenceApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _webServiceUrl;

        public YoloDetectionService(string webServiceUrl, HttpClient? httpClient = null)
        {
            _webServiceUrl = webServiceUrl ?? throw new ArgumentNullException(nameof(webServiceUrl));
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<YoloResult<YoloLabelResult>> DetectAsync(YoloImageInput input, CancellationToken ct = default)
        {
            try
            {
                if (input.Image == null || input.Image.Width == 0 || input.Image.Height == 0)
                    return YoloResult<YoloLabelResult>.Fail("INVALID_IMAGE", "Image is null or empty");

                byte[] imageBytes;
                using (var ms = new MemoryStream())
                {
                    input.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    imageBytes = ms.ToArray();
                }

                using var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(imageBytes);
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = "upload.png"
                };
                content.Add(fileContent);

                var response = await _httpClient.PostAsync(_webServiceUrl, content, ct);

                if (!response.IsSuccessStatusCode)
                {
                    return YoloResult<YoloLabelResult>.Fail("HTTP_ERROR", $"Status: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("=== RAW JSON FROM API ===");
                Console.WriteLine(responseContent);
                var json = JObject.Parse(responseContent);

                if (json["inference_result"] == null || !json["inference_result"].HasValues)
                {
                    return YoloResult<YoloLabelResult>.Fail("INVALID_RESPONSE", "Missing or empty inference_result");
                }

                var predictions = new List<RecordResult>();
                foreach (var item in json["inference_result"])
                {
                    var className = item["label"]?.ToString() ?? "unknown";
                    var classId = -1; // 如果沒有給 ClassId，就設 -1
                    var confidence = item["score"]?.ToObject<double>() ?? 0;

                    // bbox 是 [left, top, width, height]
                    var bboxArray = item["bbox"]?.ToArray();
                    int left = bboxArray?[0]?.ToObject<int>() ?? 0;
                    int top = bboxArray?[1]?.ToObject<int>() ?? 0;
                    int width = bboxArray?[2]?.ToObject<int>() ?? 0;
                    int height = bboxArray?[3]?.ToObject<int>() ?? 0;

                    var rect = new System.Drawing.Rectangle(left, top, width, height);

                    Console.WriteLine($"[推論結果] Class={className}, Score={confidence:P2}, Rect=({left},{top},{width},{height})");

                    predictions.Add(new RecordResult(className, classId, confidence, rect));
                }

                var result = new YoloLabelResult(input.ShopOrder, predictions);
                return YoloResult<YoloLabelResult>.Success(result);
            }
            catch (Exception ex)
            {
                return YoloResult<YoloLabelResult>.Fail("EXCEPTION", $"Exception during detection: {ex.Message}");
            }
        }

        public async Task<YoloResult<bool>> PingAsync(CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(_webServiceUrl, ct);
                return response.IsSuccessStatusCode
                    ? YoloResult<bool>.Success(true)
                    : YoloResult<bool>.Fail("UNAVAILABLE", "Ping failed with non-success status.");
            }
            catch (Exception ex)
            {
                return YoloResult<bool>.Fail("PING_EXCEPTION", ex.Message);
            }
        }
    }
}
