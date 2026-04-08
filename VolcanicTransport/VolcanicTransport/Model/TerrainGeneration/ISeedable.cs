namespace VolcanicTransport.Model.TerrainGeneration
{
    public interface ISeedable
    {
        public void SetSeed(int seed, Random nextRandom);
    }
}
