using System.Windows.Media;

namespace VolcanicTransport_WPF.View
{
    public readonly struct ImageWithRotation
    {
        public ImageSource ImageSource { get; }
        public int AngleDegrees { get; }

        public ImageWithRotation(ImageSource src, int deg) : this()
        {
            ImageSource = src;
            AngleDegrees = deg;
        }
    }
}
