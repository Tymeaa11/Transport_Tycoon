using VolcanicTransport.Model.TerrainGeneration;

namespace VolcanicTransport.Model.World.Economy
{
    public class Product(ProductType type, int minvalue, int maxvalue, float variability = 0.05f)
    {
        public ProductType ProductType { get; set; } = type;
        private readonly Perlin _perlin = new();
        private readonly float _minValue = minvalue;
        private readonly float _maxValue = maxvalue;
        private readonly float _variability = variability;

        public int GetDemand(float time) // min - max
            => (int)(Math.Abs(_maxValue - _minValue) * _perlin.Noise(time * _variability) + _minValue);

        public float GetFactoryEfficiency(float time)
            => _perlin.Noise(time * _variability); // 0-1

    }
}
