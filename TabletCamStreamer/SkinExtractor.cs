using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace TabletCamStreamer
{
    public class SkinExtractor
    {
        //Hsv skinLowLimit = new Hsv(0, 48, 80);
        //Hsv hsvLowBound = new Hsv(0, 0, 0);
        Hsv chromakeyLow = new Hsv(60,50,0);
        Hsv chromakeyUp = new Hsv(180,255,255);

        Hsv _hsvLowBound = new Hsv(0, 35, 30);
        Hsv _hsvUpBound = new Hsv(30, 255, 255);

        public 

        Ycc yccLowBound = new Ycc(5, 123, 60);
        Ycc yccUpBound = new Ycc(255, 180, 120);
        Image<Gray, byte> cannyImg = null;
        Image<Bgra, byte> extraction = null;
        Image<Gray, byte> skinMask = null;
        Mat dilateElm = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new System.Drawing.Size(3, 3), new System.Drawing.Point(-1, -1));
        Mat erodeElm = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new System.Drawing.Size(3, 3), new System.Drawing.Point(-1, -1));

        public double LowerHue
        {
            get => _hsvLowBound.Hue;
            set => _hsvLowBound.Hue = value;
        }
        public double LowerSaturation
        {
            get => _hsvLowBound.Satuation;
            set => _hsvLowBound.Satuation = value;
        }
        public double LowerValue
        {
            get => _hsvLowBound.Value;
            set => _hsvLowBound.Value = value;
        }
        public double UpperHue
        {
            get => _hsvUpBound.Hue;
            set => _hsvUpBound.Hue = value;
        }
        public double UpperSaturation
        {
            get => _hsvUpBound.Satuation;
            set => _hsvUpBound.Satuation = value;
        }
        public double UpperValue
        {
            get => _hsvUpBound.Value;
            set => _hsvUpBound.Value = value;
        }
        /*const int SAMPLE_SIZE = 100;
        Hsv[] sampleFrameColors = new Hsv[SAMPLE_SIZE];
        MCvScalar[] sampleColorSdvs = new MCvScalar[SAMPLE_SIZE];
        int sampleFrameCount;*/
        public SkinExtractor()
        {
            //sampleFrameCount = 0;
        }
        public void initSegmentationColorBoundaries(double hueLow,double hueUp,double satLow,double satUp,double valLow,double valUp)
        {
            LowerHue = hueLow;
            UpperHue = hueUp;
            LowerSaturation = satLow;
            UpperSaturation = satUp;
            LowerValue = valLow;
            UpperValue = valUp;
        }
        public Mat extractSkinPartUsingContours(Mat srcImg)
        {
            Image<Bgra, byte> rgbaSrc = srcImg.ToImage<Bgra, byte>();
            Image<Hsv, byte> hsvSrc = rgbaSrc.Convert<Hsv, byte>();
            extraction = rgbaSrc.Clone();
            cannyImg = hsvSrc.Canny(200, 50);
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(cannyImg, contours, null, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                double maxContourArea = 0;
                int maxCountourIdx = -1;
                for(int i=0; i< contours.Size; i++)
                {
                    VectorOfPoint contour = contours[i];
                    VectorOfPoint approxedContour = new VectorOfPoint();
                    double contourArea = CvInvoke.ContourArea(contour);
                    double periphery = CvInvoke.ArcLength(contour, true);
                    CvInvoke.ApproxPolyDP(contour, approxedContour, periphery * 0.02, true);
                    VectorOfVectorOfPoint approxedContourList = new VectorOfVectorOfPoint();
                    approxedContourList.Push(approxedContour);
                    CvInvoke.DrawContours(extraction, approxedContourList, 0, new MCvScalar(255, 255, 255, 255), -1);
                }
            }
            return extraction.Mat;
        }
        public Mat extractSkinPart(Mat srcImg)
        {
            Image<Bgra, byte> rgbaSrc = srcImg.ToImage<Bgra, byte>();
            Image<Hsv, byte> hsvSrc = rgbaSrc.Convert<Hsv, byte>();
            /*if(sampleFrameCount < SAMPLE_SIZE)
            {
                Hsv frameAvg;
                MCvScalar frameSdv;
                hsvSrc.AvgSdv(out frameAvg, out frameSdv);
                sampleFrameColors[sampleFrameCount] = frameAvg;
                sampleColorSdvs[sampleFrameCount] = frameSdv;
                sampleFrameCount++;
                if(sampleFrameCount == SAMPLE_SIZE)
                {
                    Hsv meanFrameColor = new Hsv();
                    MCvScalar meanSdv = new MCvScalar();
                    for(int i=0; i < sampleFrameCount;i++)
                    {
                        meanFrameColor.Hue += sampleFrameColors[i].Hue;
                        meanFrameColor.Satuation += sampleFrameColors[i].Satuation;
                        meanFrameColor.Value += sampleFrameColors[i].Value;

                        meanSdv.V0 += sampleColorSdvs[i].V0;
                        meanSdv.V1 += sampleColorSdvs[i].V1;
                        meanSdv.V2 += sampleColorSdvs[i].V2;
                        meanSdv.V3 += sampleColorSdvs[i].V3;
                    }
                    meanFrameColor.Hue /= sampleFrameCount;
                    meanFrameColor.Satuation /= sampleFrameCount;
                    meanFrameColor.Value /= sampleFrameCount;

                    meanSdv.V0 /= sampleFrameCount;
                    meanSdv.V1 /= sampleFrameCount;
                    meanSdv.V2 /= sampleFrameCount;
                    meanSdv.V3 /= sampleFrameCount;

                    double multitudeFactor = 5;
                    chromakeyLow.Hue = meanFrameColor.Hue - meanSdv.V0 * multitudeFactor;
                    chromakeyLow.Satuation = meanFrameColor.Satuation - meanSdv.V1 * multitudeFactor;
                    chromakeyLow.Value = meanFrameColor.Value - meanSdv.V2 * multitudeFactor;

                    chromakeyUp.Hue = meanFrameColor.Hue + meanSdv.V0 * multitudeFactor;
                    chromakeyUp.Satuation = meanFrameColor.Satuation + meanSdv.V1 * multitudeFactor;
                    chromakeyUp.Value = meanFrameColor.Value + meanSdv.V2 * multitudeFactor;
                }
            }*/
            Image<Ycc, byte> yccSrc = rgbaSrc.Convert<Ycc, byte>();
            InitMatsIfNeeded(rgbaSrc);
            //skinMask = hsvSrc.InRange(chromakeyLow, chromakeyUp);
            skinMask = hsvSrc.InRange(_hsvLowBound, _hsvUpBound);
            skinMask = skinMask.Not();
            //skinMask = yccSrc.InRange(yccLowBound, yccUpBound);
            //skinMask = skinMask.Dilate(1);
            CvInvoke.Dilate(skinMask, skinMask, dilateElm, new System.Drawing.Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar());
            CvInvoke.Erode(skinMask, skinMask, erodeElm, new System.Drawing.Point(-1, -1), 3, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar());
            skinMask = fillHolesInBinaryImage(skinMask);
            //skinMask = skinMask.Erode(1);
            CvInvoke.GaussianBlur(skinMask, skinMask, new System.Drawing.Size(5, 5), 1, 1);
            extraction.SetValue(new Bgra(0, 0, 0, 0));
            rgbaSrc.Copy(extraction, skinMask);
            return extraction.Mat;
        }
        Image<Gray, byte> fillHolesInBinaryImage(Image<Gray,byte> srcBinaryImg)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(srcBinaryImg, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            double tooSmall = srcBinaryImg.Width * srcBinaryImg.Height * 0.02;
            for(int idx=0; idx < contours.Size; idx++)
            {
                VectorOfPoint contour = contours[idx];
                VectorOfPoint approxContour = new VectorOfPoint();
                double contourLength = CvInvoke.ArcLength(contour, true);
                CvInvoke.ApproxPolyDP(contour, approxContour, 0.02 * contourLength, true);
                if(CvInvoke.ContourArea(approxContour)<tooSmall)
                {
                    VectorOfVectorOfPoint approxContourList = new VectorOfVectorOfPoint();
                    approxContourList.Push(approxContour);
                    CvInvoke.DrawContours(srcBinaryImg, approxContourList, 0, new MCvScalar(255, 255, 255, 255), -1);
                }
            }
            return srcBinaryImg;
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
