using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.TerrainGeneration
{
    public class FactoryAndCityGenerator
    {
        private float minimumDistance = 15f; // Városok és gyárak közötti minimális távolság mezőkben

        public void Generate(World.World world, int cityCount, int factoryCount)
        {
            // 1. Városok lehelyezése
            for (int i = 0; i < cityCount; i++)
            {
                Coordinate? pos = FindValidLocation(world);
                if (pos.HasValue)
                {
                    CreateCity(world, pos.Value);
                }
            }

            // 2. Gyárak lehelyezése
            for (int i = 0; i < factoryCount; i++)
            {
                Coordinate? pos = FindValidLocation(world);
                if (pos.HasValue)
                {
                    CreateFactory(world, pos.Value);
                }
            }
        }

        private Coordinate? FindValidLocation(World.World world)
        {
            int maxAttempts = 200;
            for (int i = 0; i < maxAttempts; i++)
            {
                int rx = World.World.SharedRandom.Next(2, world.SizeInFields.X - 2);
                int ry = World.World.SharedRandom.Next(2, world.SizeInFields.Y - 2);
                Coordinate potential = new Coordinate(rx, ry);

                if (IsAreaSuitable(world, potential))
                {
                    return potential;
                }
            }
            return null;
        }

        private bool IsAreaSuitable(World.World world, Coordinate center)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Coordinate current = new Coordinate(center.X + dx, center.Y + dy);
                    Field? f = world.GetField(current);

                    if (f == null || !f.IsBuildable())
                        return false;
                }
            }

            // Távolság ellenőrzése a már meglévő városoktól/gyáraktól
            foreach (var city in world.Cities)
            {
                if (GetDistance(center, city.CenterCoordinate) < minimumDistance) return false;
            }
            foreach (var factory in world.Factories)
            {
                if (GetDistance(center, factory.OriginCoordinate) < minimumDistance) return false;
            }

            return true;
        }

        private void CreateCity(World.World world, Coordinate center)
        {
            string name = "City " + (world.Cities.Count + 1);
            City newCity = new City(name, center);
            float baseHeight = world.GetField(center)?.Height ?? 0;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Coordinate c = new Coordinate(center.X + dx, center.Y + dy);
                    Field? f = world.GetField(c);
                    if (f != null)
                    {
                        f.SetFieldHeight(baseHeight);
                        f.Surface = new CityBuilding();
                        newCity.AddField(f);
                    }
                }
            }
            world.Cities.Add(newCity);
        }

        private void CreateFactory(World.World world, Coordinate origin)
        {
            int factoryType = World.World.SharedRandom.Next(0, 7);
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

            float baseHeight = world.GetField(origin)?.Height ?? 0;

            for (int dx = 0; dx < 2; dx++)
            {
                for (int dy = 0; dy < 2; dy++)
                {
                    Coordinate currentCoord = new Coordinate(origin.X + dx, origin.Y + dy);
                    Field? f = world.GetField(currentCoord);

                    if (f != null)
                    {
                        f.SetFieldHeight(baseHeight); // Kilapítás az origin magasságára
                        f.Surface = new FactoryBuilding(); // ISurface beállítása
                        newFactory.AddField(f);
                    }
                }
            }

            // 4. Hozzáadás a világlistához
            world.Factories.Add(newFactory);
        }

        private float GetDistance(Coordinate a, Coordinate b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
    }
}