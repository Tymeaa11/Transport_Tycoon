using System.Windows;
using System.Windows.Media;
using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using Vector = System.Windows.Vector;

namespace VolcanicTransport_WPF.ViewModel
{
    public class Camera : ViewModelBase
    {
        public Vector HalfScreenDimensions { private get; set; }

        private Vector _position;
        private Vector _velocity;

        private double _zoom;
        private double _zoomSpeed;

        private double _scale;
        private double _previousScale;

        private const double CameraMovementSpeed = 8000;
        private const double MovementDrag = 0.005;
        private const double CameraZoomSpeed = 6;
        private const double ZoomDrag = 0.01;
        private const double MinimumScale = 0.2;
        private const double MaximumScale = 3;
        private const double ScaleCoefficient = 0.005;


        private Vector _currentMousePosition;
        public Vector CurrentMousePosition
        {
            get => _currentMousePosition;
            set { _currentMousePosition = value; OnPropertyChanged(); }
        }

        public event EventHandler? CameraChanged;

        public Matrix ProjectionMatrix
        {
            get {
                Matrix matrix = Matrix.Identity;

                matrix.Scale(_scale, _scale);

                Vector t = _position + HalfScreenDimensions;
                matrix.Translate(t.X, t.Y);

                return matrix;
            }
        }

        public bool IsMovingUp { get; set; }
        public bool IsMovingDown { get; set; }
        public bool IsMovingLeft { get; set; }
        public bool IsMovingRight { get; set; }

        public Camera()
        {
            _position = new Vector(0, 0);
            _velocity = new Vector(0, 0);

            _scale = 1;
            CalculateZoomFromScale();
            _previousScale = _scale;

        }

        public void Update(double deltaTime)
        {
            if (IsMovingUp) _velocity.Y += CameraMovementSpeed * deltaTime;
            if (IsMovingDown) _velocity.Y -= CameraMovementSpeed * deltaTime;
            if (IsMovingLeft) _velocity.X += CameraMovementSpeed * deltaTime;
            if (IsMovingRight) _velocity.X -= CameraMovementSpeed * deltaTime;

            _position += _velocity * deltaTime;
            _velocity *= Math.Pow(MovementDrag, deltaTime);

            _zoom += _zoomSpeed * deltaTime;
            if (_zoom < 0) _zoom = 0;
            _zoomSpeed *= Math.Pow(ZoomDrag, deltaTime);

            CalculateScale();

            if (Math.Abs(_scale - _previousScale) > 0.000001)
            {
                Vector mouse = CurrentMousePosition - HalfScreenDimensions;

                _position -= mouse;
                _position *= _scale;
                _position /= _previousScale;
                _position += mouse;

                _previousScale = _scale;
            }

            OnPropertyChanged(nameof(ProjectionMatrix));
            CameraChanged?.Invoke(this, EventArgs.Empty);

        }
        
        public void PrintDebug()
        {
            System.Diagnostics.Debug.WriteLine($"p:{_position}, v:{_velocity}, s:{_scale}");
        }

        private void CalculateScale()
        {
            _scale = ScaleCoefficient * _zoom * _zoom + MinimumScale;

            if (_scale > MaximumScale)
            {
                _scale = MaximumScale;
                CalculateZoomFromScale();
            }
        }

        private void CalculateZoomFromScale() 
            => _zoom = Math.Sqrt((_scale - MinimumScale) / ScaleCoefficient);

        public void Zoom(double delta)
        {
            if (delta > 0)
                _zoomSpeed += CameraZoomSpeed;
            else
                _zoomSpeed -= CameraZoomSpeed;
        }

        public Vector ScreenToWorld(Vector screenPoint)
        {
            Vector output = new(screenPoint.X, screenPoint.Y);

            output -= _position + HalfScreenDimensions;
            output /= _scale;

            return output;
        }

        public Coordinate WorldToField(Vector worldPoint)
            => new(
                (int)Math.Floor(worldPoint.X / GameSettings.FieldSize),
                (int)Math.Floor(worldPoint.Y / GameSettings.FieldSize)
            );
        public Coordinate ScreenToField(Vector screenPoint) => WorldToField(ScreenToWorld(screenPoint));

        public Rect GetVisibleWorldBounds()
        {
            // Transform the four corners of the screen into world coordinates
            Vector topLeft = ScreenToWorld(new Vector(0, 0));
            Vector bottomRight = ScreenToWorld(HalfScreenDimensions * 2);

            return new Rect((Point)topLeft, bottomRight);
        }

    }
}
