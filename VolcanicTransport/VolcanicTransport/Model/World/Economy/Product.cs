using VolcanicTransport.Model.TerrainGeneration.Layers;

namespace VolcanicTransport.Model.World.Economy
{
    public class Product(ProductType type, float minvalue, float maxvalue, float variability = 0.05f)
    {
        #region Fields
        public ProductType ProductType { get;} = type;
        private readonly Perlin _perlin = new();
        private readonly float _minValue = minvalue;
        private readonly float _maxValue = maxvalue;
        private readonly float _variability = variability;
        #endregion
        
        #region Constructors
        public Product(Product other) : 
            this(other.ProductType, other._minValue, other._maxValue, other._variability) 
        {}
        #endregion

        #region Methods
        public int GetDemand(float time) // min - max
            => (int)(Math.Abs(_maxValue - _minValue) * _perlin.Noise(time * _variability) + _minValue);

        public float GetFactoryEfficiency(float time)
            => _perlin.Noise(time * _variability); // 0-1
        #endregion
    }
}
