using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VolcanicTransport.Model;

namespace VolcanicTransport_WPF.ViewModel
{
    public class CameraToMinimap : ViewModelBase
    {
        private Camera camera;
        public int Top => camera.WorldToField((Vector)camera.GetVisibleWorldBounds().TopLeft).Y;
        public int Left => camera.WorldToField((Vector)camera.GetVisibleWorldBounds().TopLeft).X;
        private int  Bot => camera.WorldToField((Vector)camera.GetVisibleWorldBounds().BottomRight).Y;
        private int  Right => camera.WorldToField((Vector)camera.GetVisibleWorldBounds().BottomRight).X;
        public int Width => Right - Left;
        public int Height => Bot - Top;


        public CameraToMinimap(Camera cam)
        {
            camera = cam;
            camera.PropertyChanged += Camera_PropertyChanged;
        }

        private void Camera_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Top));
            OnPropertyChanged(nameof(Left));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
        }
    }
}
