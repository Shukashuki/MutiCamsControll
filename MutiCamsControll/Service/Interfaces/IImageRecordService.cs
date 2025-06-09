using MultiCamsControl.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiCamsControl.Service
{

    public interface IImageRecordService
    {
        /// <summary>
        /// 根據人工判斷結果與推論結果，儲存影像與 CSV，並回傳處理後資訊。
        /// </summary>
        YoloResult<YoloLabelResult> Save(YoloLabel label, CancellationToken ct = default);
    }

    public class YoloLabelResult
    {
        public string ShopOrder { get; set; }
        public IReadOnlyCollection<RecordResult> Predictions { get; set; }

        public YoloLabelResult(string shopOrder, IReadOnlyCollection<RecordResult> predictions)
        {
            ShopOrder = shopOrder;
            Predictions = predictions;
        }
    }

    public class YoloImageInput
    {
        public Bitmap Image { get; set; }
        public string ShopOrder { get; set; }
        public IReadOnlyCollection<string> ExpectedClasses { get; set; }

        public YoloImageInput(Bitmap image, string shopOrder, IReadOnlyCollection<string> expectedClasses)
        {
            Image = image;
            ShopOrder = shopOrder;
            ExpectedClasses = expectedClasses;
        }
    }

    public class YoloLabel
    {
        public Bitmap Bitmap { get; }
        public string ShopOrder { get; }
        public IReadOnlyCollection<string> AiClasses { get; }
        public string ManualResult { get; }
        public IReadOnlyCollection<RecordResult> Predictions { get; }

        public YoloLabel(
            Bitmap bitmap,
            string shopOrder,
            IReadOnlyCollection<string> aiClasses,
            string manualResult,
            IReadOnlyCollection<RecordResult> predictions)
        {
            Bitmap = bitmap;
            ShopOrder = shopOrder;
            AiClasses = aiClasses;
            ManualResult = manualResult;
            Predictions = predictions;
        }
    }


    public class YoloResult<T>
    {
        public bool IsSuccess { get; set; }
        public string? ErrorCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static YoloResult<T> Success(T data) =>
            new() { IsSuccess = true, Data = data };

        public static YoloResult<T> Fail(string code, string? message = null) =>
            new() { IsSuccess = false, ErrorCode = code, Message = message };
    }

    public sealed class RecordResult
    {
        public string ClassName { get; private set; }
        public int ClassId { get; private set; }
        public double Confidence { get; private set; }
        public Rectangle BoundingBox { get; private set; }

        public RecordResult(string className, int classId, double confidence, Rectangle boundingBox)
        {
            ClassName = className;
            ClassId = classId;
            Confidence = confidence;
            BoundingBox = boundingBox;
        }
    }

    public class FakeInferenceService : IInferenceApiService
    {
        public async Task<YoloResult<YoloLabelResult>> DetectAsync(YoloImageInput input, CancellationToken ct = default)
        {
            // 模擬網路延遲，讓 UI 的「處理中」效果更真實
            await Task.Delay(TimeSpan.FromSeconds(1), ct);

            // 建立一些假的推論結果
            var fakePredictions = new List<RecordResult>();
            var random = new Random();

            // 隨機產生 1 到 3 個假物件
            int objectCount = random.Next(1, 4);
            for (int i = 0; i < objectCount; i++)
            {
                var fakeClass = "fake_object_" + (i + 1);
                var fakeConfidence = random.NextDouble() * (0.98 - 0.75) + 0.75; // 產生 0.75 到 0.98 之間的信心度
                var fakeBoundingBox = new Rectangle(
                    random.Next(0, input.Image.Width / 2),
                    random.Next(0, input.Image.Height / 2),
                    random.Next(50, input.Image.Width / 4),
                    random.Next(50, input.Image.Height / 4)
                );

                fakePredictions.Add(new RecordResult(fakeClass, i, fakeConfidence, fakeBoundingBox));
            }

            // 將假資料包裝成服務應有的回傳格式
            var fakeLabelResult = new YoloLabelResult(input.ShopOrder, fakePredictions);

            // 回傳成功的結果
            return YoloResult<YoloLabelResult>.Success(fakeLabelResult);
        }

        public Task<YoloResult<bool>> PingAsync(CancellationToken ct = default)
        {
            // 直接回傳成功
            return Task.FromResult(YoloResult<bool>.Success(true));
        }
    }
}





