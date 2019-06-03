using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletCamStreamer
{
    public class SkinExtractor
    {
        //Hsv skinLowLimit = new Hsv(0, 48, 80);
        Hsv hsvLowLimit1 = new Hsv(0, 0.2, 40);
        Hsv hsvLowLimit2 = new Hsv(150, 0.2, 40);
        Hsv hsvUpLimit1 = new Hsv(25, 0.6, 255);
        Hsv hsvUpLimit2 = new Hsv(180, 0.6, 255);
        Ycc yccLowLimit = new Ycc(0, 150, 200);
        Ycc yccUpLimit = new Ycc(255,255, 255);
        Image<Bgra, byte> extraction = null;
        Image<Gray, byte> skinMask = null;
        Image<Gray, byte> hsvMask1 = null;
        Image<Gray, byte> hsvMask2 = null;
        Image<Gray, byte> yccMask = null;
        Mat morphElm = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new System.Drawing.Size(11, 11), new System.Drawing.Point(-1, -1));
        public SkinExtractor()
        {
        }
        public Mat extractSkinPart(Mat srcImg)
        {
            Image<Hsv, byte> hsvSrc = srcImg.ToImage<Hsv, byte>();
            Image<Ycc, byte> yccSrc = srcImg.ToImage<Ycc, byte>();
            Image<Bgra, byte> rgbaSrc = srcImg.ToImage<Bgra, byte>();
            InitMatsIfNeeded(rgbaSrc);
            hsvMask1 = hsvSrc.InRange(hsvLowLimit1, hsvUpLimit1);
            hsvMask2 = hsvSrc.InRange(hsvLowLimit2, hsvUpLimit2);
            yccMask = yccSrc.InRange(yccLowLimit, yccUpLimit);
            skinMask = yccSrc.InRange(yccLowLimit, yccUpLimit);
            skinMask = hsvMask1.Or(hsvMask2);
            //skinMask = skinMask.And(yccMask);
            //CvInvoke.Dilate(skinMask, skinMask, morphElm, new System.Drawing.Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Transparent, new MCvScalar(0, 0, 0));
            //CvInvoke.Erode(skinMask, skinMask, morphElm, new System.Drawing.Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Transparent, new MCvScalar(0, 0, 0));
            extraction.SetValue(new Bgra(0, 0, 0, 0));
            //rgbaSrc.Copy(extraction, skinMask);
            return hsvMask2.Mat;
        }
        void InitMatsIfNeeded(Image<Bgra, byte> src)
        {
            if (skinMask == null)
            {
                skinMask = new Image<Gray, byte>(src.Width, src.Height);
                yccMask = new Image<Gray, byte>(src.Width, src.Height); ;
                hsvMask1 = new Image<Gray, byte>(src.Width, src.Height);
                hsvMask2 = new Image<Gray, byte>(src.Width, src.Height);
            }
            else if (skinMask.Width != src.Width || skinMask.Height != src.Height)
            {
                skinMask = new Image<Gray, byte>(src.Width, src.Height);
                yccMask = new Image<Gray, byte>(src.Width, src.Height); ;
                hsvMask1 = new Image<Gray, byte>(src.Width, src.Height);
                hsvMask2 = new Image<Gray, byte>(src.Width, src.Height);
            }
            if (extraction == null)
            {
                extraction = new Image<Bgra, byte>(src.Width, src.Height);
            }
            else if (extraction.Width != src.Width || extraction.Height != src.Height)
            {
                extraction = new Image<Bgra, byte>(src.Width, src.Height);
            }
        }
    }
}
