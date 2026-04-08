using System.Text.Json.Serialization;
using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World.Economy
{
    public class City
    {
        public string Name { get; } 
        public Coordinate CenterCoordinate { get; }
        
        public List<Product> Products { get; private set; }
        private readonly List<Field> _fields;

        [JsonConstructor]
        public City(string name, Coordinate centerCoordinate, List<Product> products)
        {
            Name = name;
            CenterCoordinate = centerCoordinate;
            _fields = [];
            Products = [];
            RandomizeNeeds();
        }
        
        public City(string name, Coordinate centerCoordinate)
        {
            Name = name;
            CenterCoordinate = centerCoordinate;
            _fields = [];
            Products = [];
            RandomizeNeeds();
        }


        public void AddField(Field f) =>_fields.Add(f);

        private void RandomizeNeeds()
        {
            var rnd = new Random();
            Products.Clear();

            for (var i = 0; i < 3; i++)
            {
                var typeIndex = rnd.Next(1, 9);

                var randomType = (ProductType)typeIndex;
                Product newProduct = new(randomType, 0, 100, 5);

                Products.Add(newProduct);
            }
        }

        public bool IsProductNeeded(ProductType productType)
            => Products.Count(f => f.ProductType == productType) != 0;

        /* TODOOO public int RecieveProduct(ProductType type, int amount)
        {
            //TODO//
            return 0;
        }*/
    }
}
