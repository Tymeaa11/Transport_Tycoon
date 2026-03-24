using VolcanicTransport.Model.World;

namespace VolcanicTransport.Model.TerrainGeneration.Generators
{
    public interface ITerrainGenerator : ISeedable
    {
        public void ModifyField(Field field, int x, int y);
    }
}
