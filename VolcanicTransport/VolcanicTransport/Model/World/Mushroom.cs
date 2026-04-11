using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World
{
    public class Mushroom( Coordinate coordinate, MushroomGrowthStage growthStage = MushroomGrowthStage.SPROUT) : KnowsNeighbour(coordinate)
    {
        public Coordinate GetCoordinate => this.Coordinate;

        public MushroomGrowthStage GrowthStage { get; private set; } = growthStage; // 1 - 4

        private bool IsAbleToSpread() => GrowthStage >= MushroomGrowthStage.ADULT;

        public void Grow()
        {
            if (GrowthStage < MushroomGrowthStage.FULLY_GROWN) GrowthStage++;
        }

        public (Coordinate?, bool) UpdateMushroom(Coordinate myCoord)
        {
            Random rand = World.Instance.SharedRandom;
            bool hasChanged = false;

            if (rand.NextDouble() < GameSettings.GrowthBaseChance) 
            {
                int oldStage = (int)GrowthStage;
                Grow();
                if ((int)GrowthStage != oldStage)
                {
                    hasChanged = true;
                    return (myCoord, hasChanged);
                }
            }

            if (IsAbleToSpread() && rand.NextDouble() < GameSettings.SpreadBaseChance)
            {
                Coordinate dir = rand.Next(4) switch
                {
                    0 => new Coordinate(0, -1), 
                    1 => new Coordinate(0, 1),  
                    2 => new Coordinate(1, 0),
                    _ => new Coordinate(-1, 0)
                };

                Coordinate targetCoord = myCoord + dir;
                Field? targetField = World.Instance.GetField(targetCoord);

                if (targetField != null && (targetField.Type is < FieldType.LOW_LANDS or > FieldType.HIGH_LANDS)) return (null, hasChanged);

                if (targetField != null && targetField.Surface == null && targetField.IsBuildable())
                {
                    targetField.Surface = new Mushroom(targetCoord, MushroomGrowthStage.SPROUT);
                    hasChanged = true;
                    return (targetCoord, hasChanged);
                }
            }

            return (null, hasChanged);
        }
    }
}
