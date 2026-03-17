namespace VolcanicTransport.Model.TerrainGeneration
{
    public abstract class ScalableLayer(float frequency, float amplitude, float offsetX, float offsetY) : ILayer
    {
        protected abstract float Calculate(float x, float y);
        
        public void SetSeed(int seed) {}
        
        public float Get(float x, float y)
            => amplitude * Calculate(frequency * (x + offsetX), frequency * (y + offsetY));
    }
}
