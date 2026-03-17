namespace VolcanicTransport.Model.TerrainGeneration
{
    public interface ILayer
    {
        public float Get(float x, float y);
        
        // Implementing SetSeed is optional
        public void SetSeed(int seed) {}
    }
}
