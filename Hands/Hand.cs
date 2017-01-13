using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using DepthTracker.Bitmaps;

namespace DepthTracker.Hands
{
    public class Hand
    {
        public bool Touch { get; private set; }
        public int Row { get; private set; }
        public int Col { get; private set; }

        public Hand(bool touch, int row, int col)
        {
            Touch = touch;
            Row = row;
            Col = col;
        }

        public static List<Hand> GetHands(WriteableBitmap writeableBitmap)
        {
            var hands = new List<Hand>();
            var phs = new palmHandSegmentation();
            var bWidth = writeableBitmap.PixelWidth / 4;
            var bHeight = writeableBitmap.PixelHeight / 2;
            for (var row = 0; row < 2; row++)
            {
                for (var col = 0; col < 4; col++)
                {
                    var bitmap = writeableBitmap.GetTile(row, col);
                    var binaryHandImage = new Image<Gray, byte>(bWidth, bHeight) { Bitmap = bitmap };

                    binaryHandImage = binaryHandImage.Rotate(30, new Gray(0));  // rotate the image to test againt rotation

                    var fullRect = new Rectangle(0, 0, binaryHandImage.Width, binaryHandImage.Height);

                    hands.Add(new Hand(phs.getPalm(binaryHandImage) != null, row, col));
                }
            }
            return hands;
        }
    }
}
