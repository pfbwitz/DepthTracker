using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace DepthTracker.Hands
{
    public class palmHandSegmentation
    {
        double minimumDistanceDepthPointToEndPointRatio = 0.15;  // adjust this to find best distance to detect finger
        double circleRadiusMultiplier = 1.1; // adjust this to set your circle size


        /* 2012
         * Coded by Shulha Yahya
         * shulha.y@gmail.com
         *  
        */
        public Image<Gray, byte> getCirclePalm(Image<Gray, byte> BinaryHandImage)
        {
            #region variable Initialisation
            Contour<Point> contours;
            Contour<Point> biggestContour = null;
            double Result1 = 0;
            double Result2 = 0;
            Seq<Point> hull;
            Seq<MCvConvexityDefect> defects;
            MCvConvexityDefect[] defectArray;
            CircleF palmHandBoundingCircle;
            MCvBox2D box;
            MemStorage storage;
            PointF[] pointsCollection;
            Image<Gray, byte> temp;
            Image<Gray, byte> result;
            int w;
            int h;
            #endregion

            w = BinaryHandImage.Width;
            h = BinaryHandImage.Height;

            temp = BinaryHandImage.CopyBlank();
            result = BinaryHandImage.CopyBlank();

            contours = BinaryHandImage.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST);
            while (contours != null)
            {
                Result1 = contours.Area;
                if (Result1 > Result2)
                {
                    Result2 = Result1;
                    biggestContour = contours;
                }
                contours = contours.HNext;
            }

            if (biggestContour != null)
            {
                biggestContour = biggestContour.ApproxPoly(0.00000001, 0, new MemStorage());
                Image<Bgr, byte> tes = BinaryHandImage.CopyBlank().Convert<Bgr, byte>();
                tes.Draw(biggestContour, new Bgr(255, 255, 255), -1);
                tes.Draw(biggestContour, new Bgr(Color.Red), 1);


                // find the palm hand area using convexityDefect
                hull = biggestContour.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);
                box = biggestContour.GetMinAreaRect();

                // find defect area
                storage = new MemStorage();
                defects = biggestContour.GetConvexityDefacts(storage, Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);
                defectArray = defects.ToArray();

                #region create points collection from ConvexityDefect depth points

                #region calculate distane between every depth point and its end Point
                int max = 0;
                int[] distance = new int[defects.Total];
                for (int i = 0; i < defects.Total; i++)
                {
                    distance[i] = (int)Math.Sqrt(Math.Pow(defectArray[i].DepthPoint.X - defectArray[i].EndPoint.X, 2) + Math.Pow(defectArray[i].DepthPoint.Y - defectArray[i].EndPoint.Y, 2));
                    max = (int)Math.Max(max, distance[i]);
                }
                #endregion

                #region find depth point that is base of the finger and assign it to importantDepthPoint
                Contour<Point> importantDepthPoint = new Contour<Point>(new MemStorage());
                int num = 0;
                for (int i = 0; i < defects.Total; i++)
                {
                    if (distance[i] > minimumDistanceDepthPointToEndPointRatio * max)
                    {
                        importantDepthPoint.Insert(num, new Point(defectArray[i].DepthPoint.X, defectArray[i].DepthPoint.Y));
                        tes.Draw(new CircleF(defectArray[i].DepthPoint, 0), new Bgr(Color.Green), 2);
                        tes.Draw(new CircleF(defectArray[i].EndPoint, 0), new Bgr(Color.Blue), 2);
                        num++;
                    }
                }
                #endregion

                pointsCollection = new PointF[importantDepthPoint.Total];
                Point[] importantDepthPointArray = importantDepthPoint.ToArray();
                for (int i = 0; i < importantDepthPoint.Total; i++)
                {
                    pointsCollection[i] = new PointF(importantDepthPointArray[i].X, importantDepthPointArray[i].Y);
                }

                #endregion

                #region find center of the palm using minimum enclosing circle, or you can chage this using central moment
                // find bounding circle from ConvexityDefect depth points collection
                palmHandBoundingCircle = PointCollection.MinEnclosingCircle(pointsCollection);
                // we treat center of the circle as the center of the palm
                // from the center of palmHandBoundingCircle, find distance to the biggestcontour and set it as the new palmHandBoundingCircle radius
                palmHandBoundingCircle = new CircleF(palmHandBoundingCircle.Center, (float)(circleRadiusMultiplier * biggestContour.Distance(palmHandBoundingCircle.Center)));
                #endregion

                // draw circle from palmHandBoundingCircle
                temp = BinaryHandImage.CopyBlank();
                temp.Draw(palmHandBoundingCircle, new Gray(255), 0);

                // perform AND operation to BinaryHandImage
                result = temp.And(BinaryHandImage);
            }

            return result;
        }

        public Image<Gray, byte> getPalm(Image<Gray, byte> BinaryHandImage)
        {
            #region variable Initialisation
            Contour<Point> contours;
            Contour<Point> biggestContour = null;
            Contour<Point> importantDepthPoint;
            double Result1 = 0;
            double Result2 = 0;
            Seq<Point> hull;
            Seq<MCvConvexityDefect> defects;
            MCvConvexityDefect[] defectArray;
            MCvBox2D box;
            MemStorage storage;
            PointF[] pointsCollection;
            Image<Gray, byte> temp;
            Image<Gray, byte> result;
            int w;
            int h;
            #endregion

            w = BinaryHandImage.Width;
            h = BinaryHandImage.Height;

            temp = BinaryHandImage.CopyBlank();
            result = BinaryHandImage.CopyBlank();

            contours = BinaryHandImage.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST);
            while (contours != null)
            {
                Result1 = contours.Area;
                if (Result1 > Result2)
                {
                    Result2 = Result1;
                    biggestContour = contours;
                }
                contours = contours.HNext;
            }

            if (biggestContour != null)
            {
                biggestContour = biggestContour.ApproxPoly(0.00000001, 0, new MemStorage());
                Image<Bgr, byte> tes = BinaryHandImage.CopyBlank().Convert<Bgr, byte>();
                tes.Draw(biggestContour, new Bgr(255, 255, 255), -1);
                tes.Draw(biggestContour, new Bgr(Color.Red), 1);


                // find the palm hand area using convexityDefect
                hull = biggestContour.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);
                box = biggestContour.GetMinAreaRect();

                // find defect area
                storage = new MemStorage();
                defects = biggestContour.GetConvexityDefacts(storage, Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);
                defectArray = defects.ToArray();

                #region create points collection from ConvexityDefect depth points

                #region calculate distane between every depth point and its end Point
                int max = 0;
                int[] distance = new int[defects.Total];
                for (int i = 0; i < defects.Total; i++)
                {
                    distance[i] = (int)Math.Sqrt(Math.Pow(defectArray[i].DepthPoint.X - defectArray[i].EndPoint.X, 2) + Math.Pow(defectArray[i].DepthPoint.Y - defectArray[i].EndPoint.Y, 2));
                    max = (int)Math.Max(max, distance[i]);
                }
                #endregion

                #region find depth point that is base of the finger and assign it to importantDepthPoint
                importantDepthPoint = new Contour<Point>(new MemStorage());
                int num = 0;
                for (int i = 0; i < defects.Total; i++)
                {
                    if (distance[i] > minimumDistanceDepthPointToEndPointRatio * max)
                    {
                        importantDepthPoint.Insert(num, new Point(defectArray[i].DepthPoint.X, defectArray[i].DepthPoint.Y));
                        tes.Draw(new CircleF(defectArray[i].DepthPoint, 0), new Bgr(Color.Green), 2);
                        tes.Draw(new CircleF(defectArray[i].EndPoint, 0), new Bgr(Color.Blue), 2);
                        num++;
                    }
                }
                #endregion

                #endregion

                // draw contour from every depth point
                temp = BinaryHandImage.CopyBlank();
                temp.Draw(importantDepthPoint, new Gray(255), -1);

                // perform AND operation to BinaryHandImage
                result = temp.And(BinaryHandImage);
            }

            return result;
        }

    }
}
