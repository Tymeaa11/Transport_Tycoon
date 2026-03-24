using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World
{
    public class Mushroom(
        Coordinate coordinate,
        MushroomGrowthStage growthStage = MushroomGrowthStage.SPROUT
        )
        : KnowsNeighbour(coordinate)
    {
        private const int SpreadChance = 50; // 0-100 %
        private static bool SpreadAttempt() => World.Instance.SharedRandom.Next(100) > SpreadChance;
        private static bool IsFieldSpreadable(Field? f) => SpreadAttempt() && f is { Surface: null };

        public MushroomGrowthStage GrowthStage { get; private set; } = growthStage; // 1 - 4

        private bool IsAbleToSpread() => GrowthStage >= MushroomGrowthStage.ADULT;

        public void Grow()
        {
            if (GrowthStage < MushroomGrowthStage.FULLY_GROWN) GrowthStage++;
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
