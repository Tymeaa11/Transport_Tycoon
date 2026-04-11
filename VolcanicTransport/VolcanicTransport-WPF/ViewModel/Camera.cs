using System.Windows;
using System.Windows.Media;
using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;

namespace VolcanicTransport_WPF.ViewModel
{
    public class Camera : ViewModelBase
    {
        private static readonly bool EnableDevMode = true;

        private const double PanSpeed = 10.0;

        private Matrix _projectionMatrix;

        public event EventHandler? CameraChanged;

        public Matrix ProjectionMatrix
        {
            get => _projectionMatrix;
            private set
            {
                _projectionMatrix = value;
                OnPropertyChanged();
                CameraChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DelegateCommand MoveUp { get; }
        public DelegateCommand MoveDown { get; }
        public DelegateCommand MoveLeft { get; }
        public DelegateCommand MoveRight { get; }

        public Camera(Matrix initialMatrix)
        {
            ProjectionMatrix = initialMatrix;

            MoveUp = new DelegateCommand(_ => Pan(0, PanSpeed));
            MoveDown = new DelegateCommand(_ => Pan(0, -PanSpeed));
            MoveLeft = new DelegateCommand(_ => Pan(PanSpeed, 0));
            MoveRight = new DelegateCommand(_ => Pan(-PanSpeed, 0));
        }

        public void Zoom(double delta, Point screenCenter)
        {
            if (!EnableDevMode) return;

            bool zoomIn = delta > 0;

            double zoomFactor = zoomIn ? 1.1 : 0.9;
            Point worldCenter = ScreenToWorld(screenCenter);
            Matrix m = ProjectionMatrix;

            // ScaleAtPrepend applies the scaling relative to the specified center point
            m.ScaleAtPrepend(zoomFactor, zoomFactor, worldCenter.X, worldCenter.Y);

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
                (int)Math.Floor(worldPoint.X / GameSettings.FieldSize),
                (int)Math.Floor(worldPoint.Y / GameSettings.FieldSize)
            );
        public Coordinate ScreenToField(Point screenPoint) => WorldToField(ScreenToWorld(screenPoint));

        public Rect GetVisibleWorldBounds(double screenWidth, double screenHeight)
        {
            // Transform the four corners of the screen into world coordinates
            Point topLeft = ScreenToWorld(new Point(0, 0));
            Point bottomRight = ScreenToWorld(new Point(screenWidth, screenHeight));

            return new Rect(topLeft, bottomRight);
        }
    }
}
