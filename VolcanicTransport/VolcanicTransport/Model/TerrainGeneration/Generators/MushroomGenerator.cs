using VolcanicTransport.Model.TerrainGeneration.Layers;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;

namespace VolcanicTransport.Model.TerrainGeneration.Generators
{
    public class MushroomGenerator : ITerrainGenerator
    {
        private readonly PerlinLayer _mushroomLayer
            = new(new Perlin(), 0.02f, 1f, World.World.Instance.SharedRandom);


        public void ModifyField(Field field, int x, int y)
        {
            var height = _mushroomLayer.Get(x, y); // 0.0-1.0 range

            if (field.Type is < FieldType.LOW_LANDS or > FieldType.HIGH_LANDS) return;
            if (field.Surface != null) return;
            switch (height)
            {
                case < GameSettings.Stage0MinHeight:
                    return;
                case < GameSettings.Stage1MinHeight:
                    field.Surface = new Mushroom(new Coordinate(x, y));
                    break;
                case < GameSettings.Stage2MinHeight:
                    field.Surface = new Mushroom(new Coordinate(x, y), MushroomGrowthStage.JUVENILE);
                    break;
                case < GameSettings.Stage3MinHeight:
                    field.Surface = new Mushroom(new Coordinate(x, y), MushroomGrowthStage.ADULT);
                    break;
                default:
                    field.Surface = new Mushroom(new Coordinate(x, y), MushroomGrowthStage.FULLY_GROWN);
                    break;
            }
        }

        public void SetSeed(int seed, Random nextRandom) => _mushroomLayer.SetSeed(seed, nextRandom);
    }
}
