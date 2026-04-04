using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World
{
    public class Mushroom( Coordinate coordinate, MushroomGrowthStage growthStage = MushroomGrowthStage.SPROUT) : KnowsNeighbour(coordinate)
    {
        public Coordinate GetCoordinate => this.Coordinate;
        private static bool SpreadAttempt() 
            => World.Instance.SharedRandom.Next(100) > GameSettings.SpreadChance;
        private static bool IsFieldSpreadable(Field? f) => SpreadAttempt() && f is { Surface: null };

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

            double growthBaseChance = 0.10;
            double spreadBaseChance = 0.05; 

            if (rand.NextDouble() < growthBaseChance) 
            {
                int oldStage = (int)GrowthStage;
                Grow();
                if ((int)GrowthStage != oldStage)
                {
                    hasChanged = true;
                    return (myCoord, hasChanged);
                }
            }

            if (IsAbleToSpread() && rand.NextDouble() < spreadBaseChance)
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

                if (targetField != null && targetField.Surface == null && targetField.IsBuildable())
                {
                    targetField.Surface = new Mushroom(targetCoord, MushroomGrowthStage.SPROUT);
                    hasChanged = true;
                    return (targetCoord, hasChanged);
                }
            }

            return (null, hasChanged);
        }


        public void Spread()
        {
            if (!IsAbleToSpread()) return;

            if (IsFieldSpreadable(North)) North!.Surface = new Mushroom(Coordinate + Direction.North);
            if (IsFieldSpreadable(South)) South!.Surface = new Mushroom(Coordinate + Direction.South);
            if (IsFieldSpreadable(East)) East!.Surface = new Mushroom(Coordinate + Direction.East);
            if (IsFieldSpreadable(West)) West!.Surface = new Mushroom(Coordinate + Direction.West);
        }

    }
}
