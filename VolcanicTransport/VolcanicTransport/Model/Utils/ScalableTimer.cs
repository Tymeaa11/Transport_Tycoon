using Timer = System.Timers.Timer;

namespace VolcanicTransport.Model.Utils
{
    public class ScalableTimer : IDisposable
    {
        private readonly Timer _timer;
        private int _timeScale;
        private readonly int _baseScale = 100;
        public bool Enabled
        {
            get => _timer.Enabled;
            set => _timer.Enabled = value;
        }

        public int TimeScale
        {
            get => _timeScale;
            set
            {
                _timeScale = Math.Clamp(value, 0, 10);

                if (_timeScale == 0)
                {
                    _timer.Stop();
                }
                else
                {
                    _timer.Interval = (double)_baseScale / _timeScale;
                    _timer.Start();
                }
            }
        }
        public event EventHandler? Elapsed;

        public ScalableTimer()
        {
            _timer = new Timer();
            _timer.Elapsed += (sender, e) =>
            {
                Elapsed?.Invoke(sender, e);
            };
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
