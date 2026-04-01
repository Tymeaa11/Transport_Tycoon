using VolcanicTransport.Model.TerrainGeneration.Layers;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;

namespace VolcanicTransport.Model.TerrainGeneration.Generators
{
    public class MushroomGenerator : ITerrainGenerator
    {
        private const float Stage0MinHeight = 0.5f;
        private const float Stage1MinHeight = 0.55f;
        private const float Stage2MinHeight = 0.6f;
        private const float Stage3MinHeight = 0.65f;

        private readonly PerlinLayer _mushroomLayer 
            = new(new Perlin(), 0.02f, 1f, World.World.Instance.SharedRandom);

        
        public void ModifyField(Field field, int x, int y)
        {
            var height = _mushroomLayer.Get(x, y); // 0.0-1.0 range

            if (field.Type is < FieldType.LOW_LANDS or > FieldType.HIGH_LANDS) return;
            if (field.Surface != null) return;
            switch (height)
            {
                case < Stage0MinHeight:
                    return;
                case < Stage1MinHeight:
                    field.Surface = new Mushroom(new Coordinate(x, y));
                    break;
                case < Stage2MinHeight:
                    field.Surface = new Mushroom(new Coordinate(x, y), MushroomGrowthStage.JUVENILE);
                    break;
                case < Stage3MinHeight:
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
