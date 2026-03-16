namespace VolcanicTransport.Model.World.Economy
{
    public class City
    {
        private string name;
        private List<Product> products;
        private List<Field> fields;

        public City(string name, List<Field> fields)
        {
            this.name = name;
            this.fields = fields;
            products = new List<Product>();
            RandomizeNeeds();
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
