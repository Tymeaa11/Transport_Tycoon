namespace VolcanicTransport.Model.TerrainGeneration.Layers
{
    public abstract class ScalableLayer : ILayer
    {
        private static float GetNewOffset(Random r) => r.NextSingle() * 1000;

        #region Fields
        private readonly float _frequency;
        private readonly float _amplitude;
        private float _offsetX;
        private float _offsetY;
        #endregion

        #region Constuctors
        protected ScalableLayer(float frequency, float amplitude, float offsetX, float offsetY)
        {
            _frequency = frequency;
            _amplitude = amplitude;
            _offsetX = offsetX;
            _offsetY = offsetY;
        }

        protected ScalableLayer(float frequency, float amplitude, Random nextRandom)
        {
            _frequency = frequency;
            _amplitude = amplitude;
            SetSeed(0, nextRandom);
        }
        #endregion

        #region Methods
        protected abstract float Calculate(float x, float y);

        public void SetSeed(int seed, Random nextRandom)
        {
            _offsetX = GetNewOffset(nextRandom);
            _offsetY = GetNewOffset(nextRandom);
        }

        public float Get(float x, float y)
            => _amplitude * Calculate(_frequency * (x + _offsetX), _frequency * (y + _offsetY));
        #endregion
    }
}
