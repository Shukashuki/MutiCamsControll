using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiCamsControl.Service
{
    

    public class ImageRecordRequest
    {
        public Bitmap Bitmap { get; }
        public string ShopOrder { get; }
        public IReadOnlyCollection<string> AiClasses { get; }
        public string ManualResult { get; }

        public ImageRecordRequest(
            Bitmap bitmap,
            string shopOrder,
            IReadOnlyCollection<string> aiClasses,
            string manualResult)
        {
            Bitmap = bitmap;
            ShopOrder = shopOrder;
            AiClasses = aiClasses;
            ManualResult = manualResult;
        }
    }
}

