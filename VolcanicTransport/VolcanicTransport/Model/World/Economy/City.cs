using System.Text.Json.Serialization;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;

namespace VolcanicTransport.Model.World.Economy
{
    public class City
    {
        public string Name { get; }
        public Coordinate CenterCoordinate { get; }

        [JsonInclude]
        private List<Product> Products { get; set; }

        public List<ProductType> ProductTypes
        {
            get
            {
                List<ProductType> l = new List<ProductType>();
                foreach (Product product in Products)
                {
                    l.Add(product.ProductType);
                }
                return l;
            }
        }

        private readonly List<Field> _fields;

        [JsonConstructor]

        public City(string name, Coordinate centerCoordinate)
        {
            Name = name;
            CenterCoordinate = centerCoordinate;
            _fields = [];
            Products = [];
            RandomizeNeeds();
        }


        public void AddField(Field f) => _fields.Add(f);

        private void RandomizeNeeds()
        {
            var rnd = new Random();
            Products.Clear();

            List<int> possibleIndexes = Enumerable.Range(2, 7).ToList();

            possibleIndexes = possibleIndexes.OrderBy(x => rnd.Next()).ToList();

            for (var i = 0; i < 3; i++)
            {
                var randomType = (ProductType)possibleIndexes[i];
                Product newProduct = new(randomType, 0, 100, 5);

                Products.Add(newProduct);
            }
        }

        public bool IsProductNeeded(ProductType? productType)
            => Products.Count(f => f.ProductType == productType) != 0;
    }
}
