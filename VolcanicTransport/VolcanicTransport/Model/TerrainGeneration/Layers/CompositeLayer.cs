namespace VolcanicTransport.Model.TerrainGeneration.Layers
{
    public class CompositeLayer(ILayer layer0, ILayer layer1, ILayer layer2) : ILayer
    {
        public float Get(float x, float y) => layer0.Get(layer1.Get(x, y), layer2.Get(x, y));

        public void SetSeed(int seed, Random nextRandom)
        {
            layer0.SetSeed(seed, nextRandom);
            layer1.SetSeed(seed, nextRandom);
            layer2.SetSeed(seed, nextRandom);
        }
    }
}
