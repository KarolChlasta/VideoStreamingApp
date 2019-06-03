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
        Hsv hsvLowLimit1 = new Hsv(0, 40, 60);
        Hsv hsvUpLimit1 = new Hsv(30, 220, 220);
        Image<Bgra, byte> extraction = null;
        Image<Gray, byte> skinMask = null;
        public SkinExtractor()
        {
        }
        public Mat extractSkinPart(Mat srcImg)
        {
            Image<Bgra, byte> rgbaSrc = srcImg.ToImage<Bgra, byte>();
            Image<Hsv, byte> hsvSrc = rgbaSrc.Convert<Hsv, byte>();
            Image<Ycc, byte> yccSrc = rgbaSrc.Convert<Ycc, byte>();
            InitMatsIfNeeded(rgbaSrc);
            skinMask = hsvSrc.InRange(hsvLowLimit1, hsvUpLimit1);
            skinMask = skinMask.Dilate(2);
            skinMask = skinMask.Erode(2);
            CvInvoke.GaussianBlur(skinMask, skinMask, new System.Drawing.Size(3, 3), 1, 1);
            extraction.SetValue(new Bgra(0, 0, 0, 0));
            rgbaSrc.Copy(extraction, skinMask);
            return extraction.Mat;
        }
        void InitMatsIfNeeded(Image<Bgra, byte> src)
        {
            if (skinMask == null)
            {
                skinMask = new Image<Gray, byte>(src.Width, src.Height);
            }
            else if (skinMask.Width != src.Width || skinMask.Height != src.Height)
            {
                skinMask = new Image<Gray, byte>(src.Width, src.Height);
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
