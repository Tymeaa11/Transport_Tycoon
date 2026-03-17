namespace VolcanicTransport.Model.TerrainGeneration
{
    public class PerlinLayer
        (Perlin perlin, float frequency, float amplitude, float offsetX, float offsetY)
        : ScalableLayer(frequency, amplitude, offsetX, offsetY)
    {
        protected override float Calculate(float x, float y) => perlin.Noise(x, y);
        public new void SetSeed(int seed) => perlin.NoiseSeed(seed);
    }
}
