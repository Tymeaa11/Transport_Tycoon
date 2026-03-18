namespace VolcanicTransport.Model.TerrainGeneration
{
    public interface ILayer
    {
        public float Get(float x, float y);

        // Implementing SetSeed is optional
        // TODO : SetSeed currently does useless work, doesn't update offsetX, offsetY
        public void SetSeed(int seed) { }
    }
}
