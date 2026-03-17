using VolcanicTransport.Model.World;

namespace VolcanicTransport.Model.TerrainGeneration
{
    public interface ITerrainGenerator
    {
        public void ModifyField(Field field, int x, int y);
        // Implementing SetSeed is optional
        public void SetSeed(int seed) {}
    }
}
