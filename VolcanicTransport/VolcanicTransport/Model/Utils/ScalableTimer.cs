using Timer = System.Timers.Timer;

namespace VolcanicTransport.Model.Utils
{
    public class ScalableTimer : IDisposable
    {
        private const int BaseScale = 100;
        
        #region Fields
        public event EventHandler? Elapsed;
        private readonly Timer _timer;
        private int _timeScale;
        #endregion
        
        #region Properties
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
                    _timer.Interval = (double)BaseScale / _timeScale;
                    _timer.Start();
                }
            }
        }
        #endregion
    
        #region Constructors
        public ScalableTimer()
        {
            _timer = new Timer();
            _timer.Elapsed += (sender, e) =>
            {
                Elapsed?.Invoke(sender, e);
            };
        }
        #endregion

        #region Methods
        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
        public void Dispose() => _timer.Dispose();
        #endregion
    }
}
