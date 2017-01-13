using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Collections.Generic;
using DepthTracker.Hands;

namespace DepthTracker.UI
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region attr

        public ImageSource ImageSource { get { return _depthBitmap; } }

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                }
            }
        }

        private const int MapDepthToByte = 8000 / 256; // Map depth range to byte range

        private KinectSensor _kinectSensor = null;

        private DepthFrameReader _depthFrameReader = null;

        private FrameDescription _depthFrameDescription = null;

        private WriteableBitmap _depthBitmap = null;

        private byte[] _depthPixels = null;

        private string _statusText = null;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public MainWindow()
        {
            _kinectSensor = KinectSensor.GetDefault();
            _depthFrameReader = _kinectSensor.DepthFrameSource.OpenReader();
            _depthFrameReader.FrameArrived += Reader_FrameArrived;
            _depthFrameDescription = _kinectSensor.DepthFrameSource.FrameDescription;
            _depthPixels = new byte[_depthFrameDescription.Width * _depthFrameDescription.Height];
            _depthBitmap = new WriteableBitmap(_depthFrameDescription.Width, _depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);

            _kinectSensor.IsAvailableChanged += Sensor_IsAvailableChanged;
            _kinectSensor.Open();

            StatusText = _kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText : 
                Properties.Resources.NoSensorStatusText;

            DataContext = this;

            InitializeComponent();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (_depthFrameReader != null)
            {
                _depthFrameReader.Dispose();
                _depthFrameReader = null;
            }
            if (_kinectSensor != null)
            {
                _kinectSensor.Close();
                _kinectSensor = null;
            }
        }

        private void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            var depthFrameProcessed = false;

            using (var depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    using (var depthBuffer = depthFrame.LockImageBuffer())
                    {
                        // verify data and write the color data to the display bitmap
                        if (((_depthFrameDescription.Width * _depthFrameDescription.Height) == (depthBuffer.Size / _depthFrameDescription.BytesPerPixel)) &&
                            (_depthFrameDescription.Width == _depthBitmap.PixelWidth) && (_depthFrameDescription.Height == _depthBitmap.PixelHeight))
                        {
                            ushort maxDepth = 1100;// ushort.MaxValue;
                            ushort minDepth = 1000;// depthFrame.DepthMinReliableDistance

                            ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, minDepth, maxDepth);
                            depthFrameProcessed = true;
                        }
                    }
                }
            }

            if (depthFrameProcessed)
            {
                RenderDepthPixels();
                var hands = GetHands();
            }
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the DepthFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the depthFrameData pointer.
        /// </summary>
        /// <param name="depthFrameData">Pointer to the DepthFrame image data</param>
        /// <param name="depthFrameDataSize">Size of the DepthFrame image data</param>
        /// <param name="minDepth">The minimum reliable depth value for the frame</param>
        /// <param name="maxDepth">The maximum reliable depth value for the frame</param>
        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / _depthFrameDescription.BytesPerPixel); ++i)
            {
                ushort depth = frameData[i]; // Get the depth for this pixel
                _depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? 255 : 0);
            }
        }

        private List<Hand> GetHands()
        {
            return _depthBitmap != null ? Hand.GetHands(_depthBitmap) : null;
        }

        private void RenderDepthPixels()
        {
            _depthBitmap.WritePixels(
                new Int32Rect(0, 0, _depthBitmap.PixelWidth, _depthBitmap.PixelHeight),
                _depthPixels,
                _depthBitmap.PixelWidth,
                0);
        }

        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            StatusText = _kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                : Properties.Resources.SensorNotAvailableStatusText;
        }
    }
}
