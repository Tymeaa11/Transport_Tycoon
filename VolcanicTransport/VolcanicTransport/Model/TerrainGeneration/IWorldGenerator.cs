using VolcanicTransport.Model.TerrainGeneration.Generators;

namespace VolcanicTransport.Model.TerrainGeneration
{
    public interface IWorldGenerator : ITerrainGenerator
    {
        public void GenerateCitiesAndFactories();
    }
}
