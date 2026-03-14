using VolcanicTransport.Model.TerrainGeneration;

namespace VolcanicTransport.Model.World.Economy
{
    public class Product
    {
       private  ProductType producType;
       private Perlin? perlin;
       private float minValue;
       private float maxValue;
       private float variability;

       public ProductType ProductType
        {
            get { return producType; }
            set { producType = value; }
        }

       public Product(ProductType type, int minvalue, int maxvalue, int variability)
        {
            this.producType = type;
            this.minValue = minvalue;
            this.maxValue = maxvalue;
            this.variability = variability;
        }

        public int GetDemand()
        {
            return 0;
        }

        public float GetFactoryEfficiency()
        {
            return 0; 
        }
    }
}
