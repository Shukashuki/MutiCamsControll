using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MultiCamsControl.Service.Interfaces;

namespace MultiCamsControl.Service
{
    public sealed class ImageRecordService: IImageRecordService
    {
        private readonly string _passRoot;
        private readonly string _failRoot;

        public ImageRecordService(string passRoot, string failRoot)
        {
            _passRoot = passRoot;
            _failRoot = failRoot;
        }

        public YoloResult<YoloLabelResult> Save(YoloLabel req, CancellationToken ct = default)
        {
            try
            {
                var baseFolder = req.ManualResult.Equals("pass", StringComparison.OrdinalIgnoreCase)
                    ? _passRoot : _failRoot;

                var workFolder = Path.Combine(baseFolder, req.ShopOrder);
                Directory.CreateDirectory(workFolder);

                var fileNameBase = $"{req.ShopOrder}_{DateTime.Now:yyyyMMdd_HHmmss}";
                var imgPath = Path.Combine(workFolder, $"{fileNameBase}.png");
                req.Bitmap.Save(imgPath, ImageFormat.Png);

                if (req.Predictions.Any())
                {
                    var csvPath = Path.Combine(workFolder, $"{fileNameBase}.csv");
                    using (var writer = new StreamWriter(csvPath, false, Encoding.UTF8))
                    {
                        writer.WriteLine("ClassName,ClassId,Confidence,Left,Top,Width,Height");

                        foreach (var r in req.Predictions)
                        {
                            var rect = r.BoundingBox;
                            writer.WriteLine($"{r.ClassName},{r.ClassId},{r.Confidence:F3},{rect.Left},{rect.Top},{rect.Width},{rect.Height}");
                        }
                    }
                }

                var result = new YoloLabelResult(req.ShopOrder, req.Predictions);
                return YoloResult<YoloLabelResult>.Success(result);
            }
            catch (Exception ex)
            {
                return YoloResult<YoloLabelResult>.Fail("SAVE_ERROR", $"Save failed: {ex.Message}");
            }
        }
    }

}
