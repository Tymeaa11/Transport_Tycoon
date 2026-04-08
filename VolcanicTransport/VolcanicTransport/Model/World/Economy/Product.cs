using System.Text.Json.Serialization;
using VolcanicTransport.Model.TerrainGeneration.Layers;

namespace VolcanicTransport.Model.World.Economy
{
    public class Product
    {
        #region Fields
        public ProductType ProductType { get;}
        private readonly Perlin _perlin = new();
        public float MinValue { get; }
        public float MaxValue { get; }
        public float Variability { get; }
        #endregion
        
        #region Constructors
        public Product(Product other) : 
            this(other.ProductType, other.MinValue, other.MaxValue, other.Variability) 
        {}

        [JsonConstructor]
        public Product(ProductType productType, float minvalue, float maxvalue, float variability = 0.05f)
        {
            ProductType = productType;
            MinValue = minvalue;
            MaxValue = maxvalue;
            Variability = variability;
        }
        #endregion

        #region Methods
        public int GetDemand(float time) // min - max
            => (int)(Math.Abs(MaxValue - MinValue) * _perlin.Noise(time * Variability) + MinValue);

        public float GetFactoryEfficiency(float time)
            => _perlin.Noise(time * Variability); // 0-1
        #endregion
    }
}
