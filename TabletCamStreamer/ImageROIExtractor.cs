using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletCamStreamer
{
    public class ImageROIExtractor
    {
        PointF[] _srcAbsCorners = new PointF[4];
        PointF[] _dstAbsCorners = new PointF[4];
        Mat warpMatrix = null;
        Size dstSize;
        public ImageROIExtractor()
        {

        }
        public void setExtractionParams(PointF[] srcRelCorners,float srcW,float srcH,float dstW,float dstH)
        {
            for(int i=0; i<srcRelCorners.Length; i++)
            {
                _srcAbsCorners[i] = new PointF(srcRelCorners[i].X * srcW, srcRelCorners[i].Y * srcH);
            }
            _dstAbsCorners[0] = new PointF(0, 0);
            _dstAbsCorners[1] = new PointF(dstW, 0);
            _dstAbsCorners[2] = new PointF(dstW, dstH);
            _dstAbsCorners[3] = new PointF(0, dstH);
            //warpMatrix = CvInvoke.GetAffineTransform(_srcAbsCorners, _dstAbsCorners);
            warpMatrix = CvInvoke.GetPerspectiveTransform(_srcAbsCorners, _dstAbsCorners);
            dstSize = new Size((int)dstW, (int)dstH);
        }
        Mat dstImg = new Mat();
        public Mat extractROI(Mat srcImg)
        {
            if(warpMatrix != null)
            {
                //CvInvoke.WarpAffine(srcImg, dstImg, warpMatrix, dstSize, Emgu.CV.CvEnum.Inter.Linear, Warp.Default, BorderType.Transparent);
                CvInvoke.WarpPerspective(srcImg, dstImg, warpMatrix, dstSize, Emgu.CV.CvEnum.Inter.Linear, Warp.Default, BorderType.Transparent);
                return dstImg;
            }
            return srcImg;
        }
    }
}
