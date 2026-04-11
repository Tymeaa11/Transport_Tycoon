using VolcanicTransport.Model.TerrainGeneration.Layers;

namespace VolcanicTransport.Model.World.Economy
{
    public class Product(ProductType type, int minvalue, int maxvalue, float variability = 0.05f)
    {
        public ProductType ProductType { get; set; } = type;
        private readonly Perlin _perlin = new Perlin(4, 0.5f);
        private readonly float _minValue = minvalue;
        private readonly float _maxValue = maxvalue;
        private readonly float _variability = variability;

        public int GetDemand(float time) // min - max
            => (int)(Math.Abs(_maxValue - _minValue) * _perlin.Noise(time * _variability) + _minValue);

        public float GetFactoryEfficiency(float time)
            => _perlin.Noise((time + 1000f) * _variability); // 0-1

        public float GetPassengerEfficiency(float time)
            => _perlin.Noise((time + 500f) * _variability);

        /// <summary>
        /// Aktuális árkalkuláció (opcionális ötlet)
        /// Ha nagy a kereslet, az ár mehet feljebb.
        /// </summary>
        public double GetCurrentPrice(float time)
        {
            float demandPercent = _perlin.Noise(time * _variability);
            // Alapár + kereslet alapú bónusz
            return 100 + (demandPercent * 50);
        }

    }
}
