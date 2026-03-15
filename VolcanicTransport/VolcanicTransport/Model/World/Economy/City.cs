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

        }
    }
}
