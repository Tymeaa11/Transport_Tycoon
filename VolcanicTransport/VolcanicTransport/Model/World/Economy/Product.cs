using System.Text.Json.Serialization;

namespace VolcanicTransport.Model.World.Economy
{
    [method: JsonConstructor]
    public class Product(ProductType productType, float minvalue, float maxvalue, float offset, float variability)
    {
        public static float GetRandomOffset() => World.Instance.SharedRandom.NextSingle() * 50_000 - 25_000;
        public float NewRandomOffset() => _perlinOffset = GetRandomOffset();

        #region Fields
        [JsonInclude]
        public ProductType ProductType { get; } = productType;

        [JsonInclude]
        private float _perlinOffset = offset;
        private float Noise(float t) => World.Instance.SharedPerlin.Noise(t + _perlinOffset);

        [JsonInclude]
        private float MinValue { get; } = minvalue;

        [JsonInclude]
        private float MaxValue { get; } = maxvalue;

        [JsonInclude]
        private float Variability { get; } = variability;

        #endregion

        #region Constructors
        public Product(Product other) :
            this(other.ProductType, other.MinValue, other.MaxValue, other._perlinOffset, other.Variability)
        { }

        public Product(ProductType productType, float minvalue, float maxvalue, float variability = 0.05f) :
            this(productType, minvalue, maxvalue, GetRandomOffset(), variability)
        { }

        #endregion

        #region Methods
        public int GetDemand(float time) // min - max
            => (int)(Math.Abs(MaxValue - MinValue) * Noise(time * Variability) + MinValue);

        public float GetFactoryEfficiency(float time)
            => Noise((time + 1000f) * Variability); // 0 - 1

        public float GetPassengerEfficiency(float time)
            => Noise((time + 500f) * Variability);

        #endregion
    }
}
