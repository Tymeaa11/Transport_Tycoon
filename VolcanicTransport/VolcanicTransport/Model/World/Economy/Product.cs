using VolcanicTransport.Model.TerrainGeneration;

namespace VolcanicTransport.Model.World.Economy
{
    public class Product
    {
       private  ProductType producType;
       private Perlin perlin;
       private float minValue;
       private float maxValue;
       private float variability;

       public ProductType ProductType
        {
            get { return producType; }
            set { producType = value; }
        }

       public Product(ProductType type, int minvalue, int maxvalue)
        {
            this.producType = type;
            this.minValue = minvalue;
            this.maxValue = maxvalue;
            this.variability = 0.05f;

            this.perlin = new Perlin();
        }

        public int GetDemand(float time) // min - max
            => (int) (Math.Abs(maxValue - minValue) * perlin.Noise(time *variability) + minValue);

        public float GetFactoryEfficiency(float time)
            => perlin.Noise(time * variability); // 0-1

    }
}
