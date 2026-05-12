using VolcanicTransport.Model;
using VolcanicTransport.Model.TerrainGeneration;
using VolcanicTransport.Model.TerrainGeneration.Generators;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.WorldTests
{
    [TestClass]
    [DoNotParallelize]
    public class WorldGenTests
    {
        public static GameWorld World => GameWorld.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            GameWorld.Initialise(8, 123);

            World.GameWorldGenerator = new GameWorldGenerator(
                new TerrainHeightGenerator(),
                new MushroomGenerator(),
                new FactoryAndCityGenerator(GameSettings.CityCount, GameSettings.FactoryCount)
            );

            World.Generate();
        }
        [TestMethod]
        public void CityAndFactoryCountTest()
        {
            Assert.AreEqual(GameSettings.CityCount, World.Cities.Count, "Incorrect number of cities generated.");
            Assert.AreEqual(GameSettings.FactoryCount, World.Factories.Count, "Incorrect number of factories generated.");
        }

        [TestMethod]
        public void MushroomAndHeightGenerationTest()
        {
            bool foundMushroom = false;
            bool allHeightsGenerated = true;

            World.ChunkMatrix.ReadEach((_, _, chunk) =>
                chunk.FieldMatrix.ReadEach((_, _, field) =>
                {
                    if (field.Surface is Mushroom) foundMushroom = true;
                    if (field.Type < FieldType.DEEP_LAVA_OCEAN || field.Type > FieldType.HIGH_MOUNTAINS) allHeightsGenerated = false;
                })
            );

            Assert.IsTrue(foundMushroom, "No mushrooms were found in the generated world.");
            Assert.IsTrue(allHeightsGenerated, "Some fields have invalid or uninitialized height values.");
        }

        [TestMethod]
        public void CityLayoutAndReferenceTest()
        {
            foreach (var city in World.Cities)
            {
                Coordinate center = city.CenterCoordinate;
                Coordinate[] roadCoords = [
                    center + Coordinate.North,
                    center + Coordinate.South,
                    center + Coordinate.East,
                    center + Coordinate.West, center
                ];

                foreach (var coord in roadCoords)
                {
                    var surface = World.GetField(coord)?.Surface;
                    Assert.IsTrue(surface is Road, $"City road missing at {coord}");
                }

                Coordinate[] cornerCoords = [
                center + Coordinate.North + Coordinate.East,
                center + Coordinate.North + Coordinate.West,
                center + Coordinate.South + Coordinate.East,
                center + Coordinate.South + Coordinate.West
            ];

                foreach (var coord in cornerCoords)
                {
                    var surface = World.GetField(coord)?.Surface;
                    Assert.IsInstanceOfType<CityBuilding>(surface, $"City building missing at corner {coord}");

                    var building = (CityBuilding)surface!;
                    Assert.AreEqual(city.Name, building.CityName, "CityBuilding has incorrect CityName reference.");
                }
            }
        }

        [TestMethod]
        public void FactoryLayoutAndReferenceTest()
        {
            foreach (var factory in World.Factories)
            {
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        Coordinate target = new(factory.OriginCoordinate.X + x, factory.OriginCoordinate.Y + y);
                        var surface = World.GetField(target)?.Surface;

                        Assert.IsInstanceOfType<FactoryBuilding>(surface, $"FactoryBuilding missing at {target}");

                        var building = (FactoryBuilding)surface!;
                        Assert.AreEqual(factory.Name, building.FactoryName, "FactoryBuilding has incorrect FactoryName reference.");
                    }
                }
            }
        }

        [TestMethod]
        public void NoStructuresInLavaOceanTest()
        {
            World.ChunkMatrix.ReadEach((_, _, chunk) =>
                chunk.FieldMatrix.ReadEach((_, _, field) =>
                {
                    if (field.Surface is CityBuilding or FactoryBuilding)
                    {
                        Assert.IsTrue(field.Type > FieldType.LAVA_OCEAN,
                            $"Structure found in Lava Ocean at field type: {field.Type}");
                    }
                })
            );
        }

        [TestMethod]
        public void NoCityFactoryOverlapTest()
        {
            HashSet<Coordinate> occupiedCoords = [];

            foreach (var factory in World.Factories)
            {
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        Coordinate target = new(factory.OriginCoordinate.X + x, factory.OriginCoordinate.Y + y);
                        Assert.IsTrue(occupiedCoords.Add(target), $"Factory overlap detected at {target}");
                    }
                }
            }

            foreach (var city in World.Cities)
            {
                Coordinate center = city.CenterCoordinate;
                Coordinate[] cityTiles = [
                    center,
                    center + Coordinate.North, center + Coordinate.South,
                    center + Coordinate.East, center + Coordinate.West,
                    center + Coordinate.North + Coordinate.East,
                    center + Coordinate.North + Coordinate.West,
                    center + Coordinate.South + Coordinate.East,
                    center + Coordinate.South + Coordinate.West
                ];

                foreach (var coord in cityTiles)
                {
                    Assert.IsTrue(occupiedCoords.Add(coord), $"City/Factory overlap detected at {coord}");
                }
            }
        }
    }
}
