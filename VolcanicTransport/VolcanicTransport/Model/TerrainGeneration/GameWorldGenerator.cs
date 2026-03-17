using VolcanicTransport.Model.World;

namespace VolcanicTransport.Model.TerrainGeneration
{
    public class GameWorldGenerator
    {
        private readonly TerrainHeightGenerator _terrainHeightGenerator;

        public GameWorldGenerator(TerrainHeightGenerator terrainHeightGenerator)
        {
            _terrainHeightGenerator = terrainHeightGenerator;
        }

        public void GenerateField(Field field, int x, int y)
        {
            //_terrainHeightGenerator.ModifyField(field, x, y);
        }
    }
}
