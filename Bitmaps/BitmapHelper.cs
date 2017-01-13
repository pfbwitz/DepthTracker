using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace DepthTracker.Bitmaps
{
    public static class BitmapHelper
    {
        public static Bitmap GetTile(this WriteableBitmap writeableBitmap, int row, int col)
        {
            var bitmap = writeableBitmap.ToBitmap();
            var width = writeableBitmap.PixelWidth / 4;
            var height = writeableBitmap.PixelHeight / 2;
            return bitmap.Clone(
                new Rectangle(col * width, row * height, width, height), 
                System.Drawing.Imaging.PixelFormat.DontCare);
        }

        public static byte[] ToByteArray(this WriteableBitmap bitmap)
        {
            var img = Image.FromHbitmap(bitmap.BackBuffer);
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                return stream.ToArray();
            }
        }

        public static Bitmap ToBitmap(this WriteableBitmap writeBmp)
        {
            using (var stream = new MemoryStream())
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(writeBmp));
                enc.Save(stream);
                return new Bitmap(stream);
            }
        }
    }
}
