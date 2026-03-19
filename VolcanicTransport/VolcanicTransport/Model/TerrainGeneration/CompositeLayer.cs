namespace VolcanicTransport.Model.TerrainGeneration
{
    public class CompositeLayer(ILayer layer0, ILayer layer1, ILayer layer2) : ILayer
    {
        public float Get(float x, float y) => layer0.Get(layer1.Get(x, y), layer2.Get(x, y));

        public void SetSeed(int seed)
        {
            layer0.SetSeed(seed);
            layer1.SetSeed(seed);
            layer2.SetSeed(seed);
        }
    }
}
