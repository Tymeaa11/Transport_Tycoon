using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class City
    {
        private readonly string _name;
        private readonly List<Product> _products;
        private readonly List<Field> _fields;

        public City(string name, Coordinate coord)
        {
            _name = name;
            CenterCoordinate = coord;
            _fields = [];
            _products = [];
            RandomizeNeeds();
        }

        public Coordinate CenterCoordinate { get; }

        public void AddField(Field f)
        {
            _fields.Add(f);
        }

        private void RandomizeNeeds()
        {
            var rnd = new Random();
            _products.Clear();

            for (int i = 0; i < 3; i++)
            {
                int typeIndex = rnd.Next(1, 9);

                ProductType randomType = (ProductType)typeIndex;
                Product newProduct = new(randomType, 0, 100, 5);

                _products.Add(newProduct);
            }
        }

        public bool IsProductNeeded(ProductType productType)
            => _products.Count(f => f.ProductType == productType) != 0;

        /* TODOOO public int RecieveProduct(ProductType type, int amount)
        {
            //TODO//
            return 0;
        }*/
    }
}
