using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Linq;
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




}
