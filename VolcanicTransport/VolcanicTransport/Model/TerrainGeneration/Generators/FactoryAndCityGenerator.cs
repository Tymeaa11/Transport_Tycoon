using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model;

namespace VolcanicTransport.Model.TerrainGeneration.Generators
{
    public class FactoryAndCityGenerator(int cityCount, int factoryCount)
    {
        public class GenerationErrorException : Exception { }

        // Városok és gyárak közötti minimális távolság mezőkben
        private const double MinimumDistance = 15.0; 

        // Keresési próbálkozások száma
        private const int MaxAttemps = 100;

        // Minimum távolság a világ szélétől
        private const int WorldEdgeBufferZone = 2;


        public int CityCount { get; } = cityCount;
        public int FactoryCount { get; } = factoryCount;

        private Field GetField(Coordinate coordinate) 
            => World.World.Instance.GetField(coordinate) ?? throw new GenerationErrorException();

        public void Generate()
        {
            // 1. Városok lehelyezése
            for (var i = 0; i < CityCount; i++)
            {
                var pos = FindValidLocation();
                if (pos.HasValue)
                    CreateCity(pos.Value);
            }

            // 2. Gyárak lehelyezése
            for (var i = 0; i < FactoryCount; i++)
            {
                var pos = FindValidLocation();
                if (pos.HasValue)
                    CreateFactory(pos.Value);
            }
        }

        private Coordinate? FindValidLocation()
        {
            Coordinate PossibleArea = World.World.Instance.SizeInFields - WorldEdgeBufferZone;

            for (int i = 0; i < MaxAttemps; i++)
            {
                Coordinate potential = new(
                     World.World.Instance.SharedRandom.Next(WorldEdgeBufferZone, PossibleArea.X),
                     World.World.Instance.SharedRandom.Next(2, PossibleArea.Y)
                    );

                if (IsAreaSuitable(potential))
                    return potential;
            }
            return null;
        }

        private bool IsAreaSuitable(Coordinate center)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if (!GetField(new(center.X + dx, center.Y + dy)).IsBuildable())
                        return false;

            // Távolság ellenőrzése a már meglévő városoktól/gyáraktól
            foreach (var city in World.World.Instance.Cities)
                if (center.Distance(city.CenterCoordinate) < MinimumDistance) 
                    return false;

            foreach (var factory in World.World.Instance.Factories)
                if (center.Distance(factory.OriginCoordinate) < MinimumDistance) 
                    return false;

            return true;
        }

        private void CreateCity(Coordinate center)
        {
            string name = "City " + (World.World.Instance.Cities.Count + 1);
            City newCity = new(name, center);
            Field reference = GetField(center);
            var fields = World.World.Instance.GetArea(center - 1, center + 1);

            if (fields.Count != 9) throw new GenerationErrorException();

            fields.ForEach(f => f.SetFieldTypeTo(reference));

            fields[0].Surface = new CityBuilding();
            newCity.AddField(fields[0]);

            fields[2].Surface = new CityBuilding();
            newCity.AddField(fields[0]);

            fields[6].Surface = new CityBuilding();
            newCity.AddField(fields[0]);

            fields[8].Surface = new CityBuilding();
            newCity.AddField(fields[0]);

            fields[1].Surface = new Road(center + Direction.North);
            fields[3].Surface = new Road(center + Direction.West);
            fields[4].Surface = new Road(center);
            fields[5].Surface = new Road(center + Direction.East);
            fields[7].Surface = new Road(center + Direction.South);


            fields.ForEach(f =>
            {
                if (f.Surface is Road r)
                {
                    r.RoadLayoutChanged += GameModel.Instance.OnRoadBecameJunction;
                    r.Update();
                }
            });

            World.World.Instance.Cities.Add(newCity);
        }

        private void CreateFactory(Coordinate origin)
        {
            int factoryType = World.World.Instance.SharedRandom.Next(0, 7);
            Factory newFactory = factoryType switch
            {
                0 => new CondensatorFactory(origin),
                1 => new ConcreteFactory(origin),
                2 => new SulfurProducer(origin),
                3 => new BoneProducer(origin),
                4 => new AshProducer(origin),
                5 => new MushroomProducer(origin),
                6 => new SteamProducer(origin),
                _ => new MushroomProducer(origin)
            };

            Field reference = GetField(origin);

            var fields = World.World.Instance.GetArea(origin, origin + 1);

            if (fields.Count != 4) throw new GenerationErrorException();

            fields.ForEach(f => {
                f.SetFieldTypeTo(reference); // Kilapítás az origin magasságára
                f.Surface = new FactoryBuilding(); // ISurface beállítása
                newFactory.AddField(f);
            });

            // 4. Hozzáadás a világlistához
            World.World.Instance.Factories.Add(newFactory);
        }

    }
}