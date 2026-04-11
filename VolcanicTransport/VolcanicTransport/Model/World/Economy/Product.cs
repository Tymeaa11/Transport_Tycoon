using System.Text.Json.Serialization;
using VolcanicTransport.Model.TerrainGeneration.Layers;

namespace VolcanicTransport.Model.World.Economy
{
    [method: JsonConstructor]
    public class Product(ProductType productType, float minvalue, float maxvalue, float variability = 0.05f)
    {
        #region Fields
        public ProductType ProductType { get; } = productType;
        private readonly Perlin _perlin = new(4, 0.5f);

        [JsonInclude]
        private float MinValue { get; } = minvalue;

        [JsonInclude]
        private float MaxValue { get; } = maxvalue;

        [JsonInclude]
        private float Variability { get; } = variability;

        #endregion

        #region Constructors
        public Product(Product other) :
            this(other.ProductType, other.MinValue, other.MaxValue, other.Variability)
        { }

        #endregion

        #region Methods
        public int GetDemand(float time) // min - max
            => (int)(Math.Abs(MaxValue - MinValue) * _perlin.Noise(time * Variability) + MinValue);

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
        #endregion
    }
}
