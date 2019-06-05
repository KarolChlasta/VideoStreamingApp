using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TabletCamStreamer
{
    /// <summary>
    /// Interaction logic for CustomFrameViewer.xaml
    /// </summary>
    public partial class CustomFrameViewer : UserControl
    {
        enum ViewerMode { NORMAL, CALIBRATE }
        private CamRetriever _mainCamRetriever;
        public CamRetriever MainCamRetriever
        {
            get
            {
                return _mainCamRetriever;
            }

            set
            {
                _mainCamRetriever = value;
            }
        }
        int curCamIndex = 0;
        CropCorner[] cropCorners = null;
        CropCorner _beingSelectedCorner = null;
        const int initFrameWidth = 640;
        const int initFrameHeight = 480;
        const int initDstFrameW = 100;
        const int initDstFrameH = 150;
        ViewerMode curMode;
        //Bitmap currentFrame;
        public CustomFrameViewer()
        {
            InitializeComponent();
            switchMode(ViewerMode.NORMAL);
            startMainCam();
        }

        ImageROIExtractor prevROIExtractor;
        private void btnCalib_Click(object sender, RoutedEventArgs e)
        {
            switchMode(ViewerMode.CALIBRATE);
            prevROIExtractor = _mainCamRetriever.RoiExtractor;
            _mainCamRetriever.RoiExtractor = null;
            if(cropCorners == null)
            {
                cropCorners = new CropCorner[4];
                cropCorners[0] = new CropCorner("TopLeft");
                cropCorners[0].setPosition(0, 0, initFrameWidth, initFrameHeight);
                cropCorners[1] = new CropCorner("TopRight");
                cropCorners[1].setPosition(initFrameWidth, 0, initFrameWidth, initFrameHeight);
                cropCorners[2] = new CropCorner("BottomRight");
                cropCorners[2].setPosition(initFrameWidth, initFrameHeight, initFrameWidth, initFrameHeight);
                cropCorners[3] = new CropCorner("BottomLeft");
                cropCorners[3].setPosition(0, initFrameHeight, initFrameWidth, initFrameHeight);
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            switchMode(ViewerMode.NORMAL);
            _mainCamRetriever.RoiExtractor = new ImageROIExtractor();
            int dstW = initDstFrameW;
            if (string.IsNullOrEmpty(tbDstWidth.Text) || string.IsNullOrWhiteSpace(tbDstWidth.Text))
            {
                 Int32.TryParse(tbDstWidth.Text,out dstW);
            }
            int dstH = initDstFrameH;
            if (string.IsNullOrEmpty(tbDstHeight.Text) || string.IsNullOrWhiteSpace(tbDstHeight.Text))
            {
                 Int32.TryParse(tbDstHeight.Text,out dstH);
            }
            BitmapImage curFrame = (BitmapImage)imFramePreview.Source;
            PointF[] relROICorners = new PointF[4];
            for(int i=0; i<cropCorners.Length; i++)
            {
                relROICorners[i] = new PointF(cropCorners[i].RelX, cropCorners[i].RelY);
            }
            _mainCamRetriever.RoiExtractor.setExtractionParams(relROICorners, curFrame.PixelWidth, curFrame.PixelHeight, dstW, dstH);
            _mainCamRetriever.SkinExtractor = new SkinExtractor();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _mainCamRetriever.RoiExtractor = prevROIExtractor;
            switchMode(ViewerMode.NORMAL);
        }
        void switchMode(ViewerMode mode)
        {
            if(mode == ViewerMode.CALIBRATE)
            {
                btnCalib.Visibility = Visibility.Hidden;
                btnOk.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Visible;
                tbDstWidth.IsEnabled = true;
                tbDstHeight.IsEnabled = true;
            }
            else if(mode == ViewerMode.NORMAL)
            {
                btnCalib.Visibility = Visibility.Visible;
                btnOk.Visibility = Visibility.Hidden;
                btnCancel.Visibility = Visibility.Hidden;
                tbDstWidth.IsEnabled = false;
                tbDstHeight.IsEnabled = false;
            }
            curMode = mode;
        }
        public void showVideoFrame(Bitmap frameBmp)
        {
            
            //currentFrame = frameBmp;
            Action displayaction = delegate
            {
                BitmapImage imgSrc = Utilities.ToBitmapImage(frameBmp, ImageFormat.Png);
                imFramePreview.Source = imgSrc;
            };
            imFramePreview.Dispatcher.Invoke(displayaction);
        }
        public void changeCamera(int camIndex)
        {
            curCamIndex = camIndex;
            startMainCam();
        }
        void startMainCam()
        {
            _mainCamRetriever?.Close();
            int camIndex = curCamIndex >= 0 ? curCamIndex : 0;
            //_mainCamRetriever = new CamRetriever(camIndex, 1280, 720);
            _mainCamRetriever = new CamRetriever(camIndex, initFrameWidth, initFrameHeight);
            _mainCamRetriever.NewFrameAvailableEvent += MainCamRetriever_NewFrameAvailableEvent;
            _mainCamRetriever.Start();
        }

        private void MainCamRetriever_NewFrameAvailableEvent(object sender, Bitmap frameData)
        {
            if(curMode == ViewerMode.CALIBRATE)
            {
                frameData = drawCropRect(frameData);
            }
            showVideoFrame(frameData);
        }

        public void Close()
        {
            _mainCamRetriever?.Close();
            _mainCamRetriever = null;
        }
        #region process crop area
        System.Drawing.Pen cropLinePen = null;
        System.Drawing.Brush cropCornerBrush = null;
        Bitmap drawCropRect(Bitmap src)
        {
            if(cropLinePen == null)
            {
                cropLinePen = new System.Drawing.Pen(System.Drawing.Color.Yellow);
                cropLinePen.Width = 3;
            }
            if(cropCornerBrush == null)
            {
                cropCornerBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(100,255,255,0));
            }
            using (Graphics g = Graphics.FromImage(src))
            {
                Font font = new Font("Arial", 8);
                for (int i = 0; i < cropCorners.Length; i++)
                {
                    CropCorner curCorner = cropCorners[i];
                    int nextCornerIndex = i + 1 < cropCorners.Length ? i + 1 : 0;
                    CropCorner nextCorner = cropCorners[nextCornerIndex];
                    g.DrawLine(cropLinePen, new PointF(curCorner.AbsX, curCorner.AbsY), new PointF(nextCorner.AbsX, nextCorner.AbsY));
                    g.FillRectangle(cropCornerBrush, curCorner.HitBoundary);
                    g.DrawString(curCorner.Name, font, cropCornerBrush, curCorner.NameBound.Left, curCorner.NameBound.Top);
                }
            }
            return src;
        }
        System.Drawing.Point prevMousePos;
        private void imFramePreview_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Drawing.Point mousePosOnBitmap = getMousePosRelativeToFrame(e);
            for(int i=0; i < cropCorners.Length; i++)
            {
                if(cropCorners[i].isUnderMouse(mousePosOnBitmap))
                {
                    _beingSelectedCorner = cropCorners[i];
                    break;
                }
            }
            prevMousePos = mousePosOnBitmap;
        }

        private void imFramePreview_MouseMove(object sender, MouseEventArgs e)
        {
            System.Drawing.Point mousePosOnBitmap = getMousePosRelativeToFrame(e);
            if(_beingSelectedCorner != null)
            {
                BitmapImage bmpSource = (BitmapImage)imFramePreview.Source;
                int difX = mousePosOnBitmap.X - prevMousePos.X;
                int difY = mousePosOnBitmap.Y - prevMousePos.Y;
                _beingSelectedCorner.setPosition(_beingSelectedCorner.AbsX + difX, _beingSelectedCorner.AbsY + difY, bmpSource.PixelWidth, bmpSource.PixelHeight);
                prevMousePos = mousePosOnBitmap;
            }
        }

        private void imFramePreview_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _beingSelectedCorner = null;
        }
        System.Drawing.Point getMousePosRelativeToFrame(MouseEventArgs e)
        {
            BitmapImage bmpSource = (BitmapImage)imFramePreview.Source;
            System.Drawing.Point posOnBitmap = new System.Drawing.Point();
            posOnBitmap.X = (int)(e.GetPosition(imFramePreview).X * bmpSource.PixelWidth / imFramePreview.ActualWidth);
            posOnBitmap.Y = (int)(e.GetPosition(imFramePreview).Y * bmpSource.PixelHeight / imFramePreview.ActualHeight);
            return posOnBitmap;
        }
        #endregion


    }
}
