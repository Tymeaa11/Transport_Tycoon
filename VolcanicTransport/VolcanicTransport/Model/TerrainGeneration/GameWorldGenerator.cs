using VolcanicTransport.Model.TerrainGeneration.Generators;
using VolcanicTransport.Model.World;

namespace VolcanicTransport.Model.TerrainGeneration
{
    public class GameWorldGenerator(
        TerrainHeightGenerator terrainHeightGenerator,
        MushroomGenerator mushroomGenerator,
        FactoryAndCityGenerator factoryAndCityGenerator
        ) : IWorldGenerator
    {

        public void ModifyField(Field field, int x, int y)
        {
            terrainHeightGenerator.ModifyField(field, x, y);
            mushroomGenerator.ModifyField(field, x, y);
        }

        public void GenerateCitiesAndFactories()
            => factoryAndCityGenerator.Generate();

        public void SetSeed(int seed, Random nextRandom)
        {
            terrainHeightGenerator.SetSeed(seed, nextRandom);
            mushroomGenerator.SetSeed(seed, nextRandom);
        }
    }
}
