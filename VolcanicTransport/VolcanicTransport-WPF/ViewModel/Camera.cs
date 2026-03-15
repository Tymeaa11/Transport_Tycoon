using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_WPF.ViewModel
{
    public class Camera : ViewModelBase
    {
        private const double PanSpeed = 10.0;
        private Matrix _projectionMatrix;

        public Matrix ProjectionMatrix 
        { 
            get => _projectionMatrix;
            private set { _projectionMatrix = value; OnPropertyChanged(); }
        }

        public DelegateCommand MoveUp { get; }
        public DelegateCommand MoveDown { get; }
        public DelegateCommand MoveLeft { get; }
        public DelegateCommand MoveRight { get; }
        //public DelegateCommand ZoomIn { get; }
        //public DelegateCommand ZoomOut { get; }

        public Camera(Matrix initialMatrix)
        {
            ProjectionMatrix = initialMatrix;

            MoveUp    = new DelegateCommand(_ => Pan(0, PanSpeed));
            MoveDown  = new DelegateCommand(_ => Pan(0, -PanSpeed));
            MoveLeft  = new DelegateCommand(_ => Pan(PanSpeed, 0));
            MoveRight = new DelegateCommand(_ => Pan(-PanSpeed, 0));

            //ZoomIn    = new DelegateCommand(_ => Zoom(120, new Point(400, 300))); // Default to center
            //ZoomOut   = new DelegateCommand(_ => Zoom(-120, new Point(400, 300)));
        }

        public void Zoom(double delta, Point mousePosition)
        {
            double zoomFactor = delta > 0 ? 1.1 : 0.9;
            Matrix m = ProjectionMatrix;
            m.ScaleAtPrepend(zoomFactor, zoomFactor, mousePosition.X, mousePosition.Y);
            ProjectionMatrix = m;
        }

        private void Pan(double dx, double dy)
        {
            Matrix m = ProjectionMatrix;
            m.Translate(dx, dy);
            ProjectionMatrix = m;
        }
        public Point ScreenToWorld(Point screenPoint)
        {
            Matrix inverted = ProjectionMatrix;
            inverted.Invert();
            return inverted.Transform(screenPoint);
        }

        public Coordinate WorldToField(Point worldPoint)
            => new(
                (int)Math.Floor(worldPoint.X / Field.FieldSize),
                (int)Math.Floor(worldPoint.Y / Field.FieldSize)
            );
        public Coordinate ScreenToField(Point screenPoint) => WorldToField(ScreenToWorld(screenPoint));
    }
}
