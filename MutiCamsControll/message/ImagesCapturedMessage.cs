using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiCamsControl.message
{
    public class ImagesCapturedMessage : ValueChangedMessage<IReadOnlyList<Bitmap>>
    {
        public ImagesCapturedMessage(IReadOnlyList<Bitmap> images) : base(images) { }
    }
}
