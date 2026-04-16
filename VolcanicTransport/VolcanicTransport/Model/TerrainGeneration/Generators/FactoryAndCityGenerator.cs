using VolcanicTransport.Model.Exceptions;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.TerrainGeneration.Generators
{
    public class FactoryAndCityGenerator(int cityCount, int factoryCount)
    {
        private static Field GetField(Coordinate coordinate)
            => World.World.Instance.GetField(coordinate) ?? throw new GenerationErrorException();

        public void Generate()
        {
            // 1. Városok lehelyezése
            for (var i = 0; i < cityCount; i++)
            {
                var pos = FindValidLocation();
                if (pos.HasValue)
                    CreateCity(pos.Value);
            }

            // 2. Gyárak lehelyezése
            for (var i = 0; i < factoryCount; i++)
            {
                var pos = FindValidLocation();
                if (pos.HasValue)
                    CreateFactory(pos.Value);
            }
        }

        private static Coordinate? FindValidLocation()
        {
            var possibleArea = World.World.Instance.SizeInFields - GameSettings.WorldEdgeBufferZone;

            for (var i = 0; i < GameSettings.MaxAttempts; i++)
            {
                Coordinate potential = new(
                     World.World.Instance.SharedRandom.Next(GameSettings.WorldEdgeBufferZone, possibleArea.X),
                     World.World.Instance.SharedRandom.Next(2, possibleArea.Y)
                    );

                if (IsAreaSuitable(potential))
                    return potential;
            }
            return null;
        }

        private static bool IsAreaSuitable(Coordinate center)
        {
            for (var dx = -1; dx <= 1; dx++)
                for (var dy = -1; dy <= 1; dy++)
                    if (!GetField(new Coordinate(center.X + dx, center.Y + dy)).IsBuildable())
                        return false;

            // Távolság ellenőrzése a már meglévő városoktól/gyáraktól
            return World.World.Instance.Cities.All(city => !(center.Distance(city.CenterCoordinate) < GameSettings.MinimumDistanceInFields))
                   && World.World.Instance.Factories.All(factory => !(center.Distance(factory.OriginCoordinate) < GameSettings.MinimumDistanceInFields));
        }

        private static void CreateCity(Coordinate center)
        {
            var name = "City " + (World.World.Instance.Cities.Count + 1);
            City newCity = new(name, center);
            var reference = GetField(center);
            var fields = World.World.Instance.GetArea(center - 1, center + 1);

            if (fields.Count != 9) throw new GenerationErrorException();

            fields.ForEach(f => f.SetFieldTypeTo(reference));

            fields[0].Surface = new CityBuilding(newCity);
            newCity.AddField(fields[0]);

            fields[2].Surface = new CityBuilding(newCity);
            newCity.AddField(fields[0]);

            fields[6].Surface = new CityBuilding(newCity);
            newCity.AddField(fields[0]);

            fields[8].Surface = new CityBuilding(newCity);
            newCity.AddField(fields[0]);

            fields[1].Surface = new Road(center + Direction.North);
            fields[3].Surface = new Road(center + Direction.West);
            fields[4].Surface = new Road(center);
            fields[5].Surface = new Road(center + Direction.East);
            fields[7].Surface = new Road(center + Direction.South);


            fields.ForEach(f =>
            {
                switch (f.Surface)
                {
                    case Road r:
                        r.RoadLayoutChanged += GameModel.OnRoadBecameJunction;
                        r.Update();
                        break;
                }
            });

            World.World.Instance.Cities.Add(newCity);
        }

        private readonly List<int> _factoryTypesToGenerate = [];

        private void FillFactoriesToGenerate()
        {
            _factoryTypesToGenerate.Clear();
            _factoryTypesToGenerate.AddRange([.. Enumerable.Range(0, 7)]);
        }

        private void CreateFactory(Coordinate origin)
        {
            if (_factoryTypesToGenerate.Count == 0) 
                FillFactoriesToGenerate();

            var name = "Factory" + World.World.Instance.SharedRandom.Next() + "_" + World.World.Instance.SharedRandom.Next();
            var factoryType = World.World.Instance.SharedRandom.Next(0, _factoryTypesToGenerate.Count);
            Factory newFactory = _factoryTypesToGenerate[factoryType] switch
            {
                0 => new CondensatorFactory(name, origin),
                1 => new ConcreteFactory(name, origin),
                2 => new SulfurProducer(name, origin),
                3 => new BoneProducer(name, origin),
                4 => new AshProducer(name, origin),
                5 => new MushroomProducer(name, origin),
                6 => new SteamProducer(name, origin),
                _ => new MushroomProducer(name, origin)
            };

            _factoryTypesToGenerate.RemoveAt(factoryType);

            //Debug.WriteLine(newFactory);

            var reference = GetField(origin);

            var fields = World.World.Instance.GetArea(origin, origin + 1);

            if (fields.Count != 4) throw new GenerationErrorException();

            fields.ForEach(f =>
            {
                f.SetFieldTypeTo(reference); // Kilapítás az origin magasságára
                f.Surface = new FactoryBuilding(newFactory); // ISurface beállítása
                newFactory.AddField(f);
            });

            // 4. Hozzáadás a világlistához
            World.World.Instance.Factories.Add(newFactory);
        }

    }
}