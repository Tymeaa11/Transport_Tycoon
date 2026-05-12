using VolcanicTransport.Model.TerrainGeneration.Generators;

namespace VolcanicTransport.Model.TerrainGeneration
{
    public interface IWorldGenerator : ISeedable, ITerrainGenerator
    {
        public void GenerateCitiesAndFactories();
    }
}
