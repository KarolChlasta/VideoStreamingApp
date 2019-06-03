using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletCamStreamer
{
    public class CropCorner
    {
        string _name;
        float _relX;
        float _relY;
        int _absX;
        int _absY;
        Rectangle _hitBoundary;
        Rectangle _nameBound;

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        public float RelX
        {
            get
            {
                return _relX;
            }

            set
            {
                _relX = value;
            }
        }

        public float RelY
        {
            get
            {
                return _relY;
            }

            set
            {
                _relY = value;
            }
        }

        public int AbsX
        {
            get
            {
                return _absX;
            }

            set
            {
                _absX = value;
            }
        }

        public int AbsY
        {
            get
            {
                return _absY;
            }

            set
            {
                _absY = value;
            }
        }

        public Rectangle HitBoundary
        {
            get
            {
                return _hitBoundary;
            }

            set
            {
                _hitBoundary = value;
            }
        }

        public Rectangle NameBound
        {
            get
            {
                return _nameBound;
            }

            set
            {
                _nameBound = value;
            }
        }

        public CropCorner(string cornerName)
        {
            _name = cornerName;
            _hitBoundary = new Rectangle();
            _nameBound = new Rectangle();
        }
        public void setPosition(int absX,int absY,int canvasW,int canvasH)
        {
            _absX = absX;
            _absY = absY;
            _relX = _absX*1.0f / canvasW;
            _relY = _absY * 1.0f / canvasH;
            _hitBoundary.X = _absX - 25;
            _hitBoundary.Y = _absY - 25;
            _hitBoundary.Width = 50;
            _hitBoundary.Height = 50;
            _nameBound.X = _hitBoundary.Left - 10;
            _nameBound.Y = _hitBoundary.Top - 25;
            _nameBound.Width = 150;
            _nameBound.Height = 45;
        }
        public bool isUnderMouse(Point mousePos)
        {
            return _hitBoundary.Contains(mousePos);
        }
    }
}
