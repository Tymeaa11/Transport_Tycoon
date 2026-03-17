using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class City
    {
        private string name;
        private List<Product> products;
        private Coordinate centerCoordinate;
        private List<Field> fields;

        public City(string name, Coordinate coord)
        {
            this.name = name;
            this.centerCoordinate = coord;
            this.fields = new List<Field>();
            products = new List<Product>();
            RandomizeNeeds();
        }

        public Coordinate CenterCoordinate { get { return centerCoordinate; } }

        public void AddField(Field f)
        {
            fields.Add(f);
        }

        private void RandomizeNeeds()
        {
            Random rnd = new Random();
            products.Clear();

            for (int i = 0; i < 3; i++)
            {
                int typeIndex = rnd.Next(1, 9);

                ProductType randomType = (ProductType)typeIndex;
                Product newProduct = new Product(randomType, 0, 100, 5);

                products.Add(newProduct);
            }
        }

        public bool IsProductNeeded(ProductType productType)
        {
            if (products.Where(f => f.ProductType == productType).Count() == 0) { return false; }
            return true;
        }

        public int RecieveProduct(ProductType type, int amount)
        {
            //TODO//
            return 0;
        }
    }
}
