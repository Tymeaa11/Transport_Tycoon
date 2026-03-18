using VolcanicTransport.Model.World;

namespace VolcanicTransport.Model.TerrainGeneration
{
    public class GameWorldGenerator
    {
        private readonly TerrainHeightGenerator _terrainHeightGenerator;
        private readonly MushroomGenerator _mushroomGenerator;

        public GameWorldGenerator(TerrainHeightGenerator terrainHeightGenerator, MushroomGenerator mushroomGenerator)
        {
            _terrainHeightGenerator = terrainHeightGenerator;
            _mushroomGenerator = mushroomGenerator;
        }

        public void GenerateField(Field field, int x, int y)
        {
            _terrainHeightGenerator.ModifyField(field, x, y);
            _mushroomGenerator.ModifyField(field, x, y);
        }
    }
}
