using VolcanicTransport.Model.World;

namespace VolcanicTransport.Model.TerrainGeneration
{
    public class GameWorldGenerator(TerrainHeightGenerator terrainHeightGenerator, MushroomGenerator mushroomGenerator)
    {
        public void GenerateField(Field field, int x, int y)
        {
            terrainHeightGenerator.ModifyField(field, x, y);
            mushroomGenerator.ModifyField(field, x, y);
        }
    }
}
