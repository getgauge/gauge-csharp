using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Gauge.CSharp.Lib;
using Google.ProtocolBuffers;

namespace Gauge.CSharp.Runner
{
    public class DefaultScreenGrabber : IScreenGrabber
    {
        public byte[] TakeScreenShot()
        {
            var bounds = Screen.GetBounds(Point.Empty);
            using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                var memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, ImageFormat.Png);
                var takeScreenshot = ByteString.CopyFrom(memoryStream.ToArray());
                return takeScreenshot.ToByteArray();
            }
        }
    }
}