namespace VolcanicTransport.Model.TerrainGeneration.Layers
{
    public class PerlinLayer : ScalableLayer
    {
        #region Fields
        private readonly Perlin _perlin;
        #endregion

        #region Constructors
        public PerlinLayer(Perlin perlin, float frequency, float amplitude, float offsetX, float offsetY) 
            : base(frequency, amplitude, offsetX, offsetY) 
        {
            _perlin = perlin;
        }

        public PerlinLayer(Perlin perlin, float frequency, float amplitude, Random nextRandom) 
            : base(frequency, amplitude, nextRandom) 
        { 
            _perlin = perlin;
        }
        #endregion

        #region Methods
        protected override float Calculate(float x, float y) => _perlin.Noise(x, y);
        #endregion
    }
}
