using VolcanicTransport.Model;
using VolcanicTransport.Model.TerrainGeneration;
using VolcanicTransport.Model.TerrainGeneration.Generators;
using VolcanicTransport.Model.Utils;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.WorldTests
{
    [TestClass]
    [DoNotParallelize]
    public class GameWorldGeneratorTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(4, 7);

        [TestMethod]
        public void ModifyField_DoesNotThrow()
        {
            var gen = new GameWorldGenerator(
                new TerrainHeightGenerator(),
                new MushroomGenerator(),
                new FactoryAndCityGenerator(1, 1)
            );
            var field = GameWorld.Instance.GetField(new Coordinate(0, 0));
            gen.ModifyField(field!, 0, 0);
        }

        [TestMethod]
        public void SetSeed_DoesNotThrow()
        {
            var gen = new GameWorldGenerator(
                new TerrainHeightGenerator(),
                new MushroomGenerator(),
                new FactoryAndCityGenerator(1, 1)
            );
            gen.SetSeed(42, new Random(42));
        }

        [TestMethod]
        public void GenerateCitiesAndFactories_DoesNotThrow()
        {
            GameWorld.Initialise(4, 8);
            var gen = new GameWorldGenerator(
                new TerrainHeightGenerator(),
                new MushroomGenerator(),
                new FactoryAndCityGenerator(2, 2)
            );
            gen.SetSeed(8, new Random(8));
            GameWorld.Instance.ChunkMatrix.ModifyEach((x, y, chunk) =>
            {
                chunk.FieldMatrix.ModifyEach((fx, fy, field) =>
                {
                    gen.ModifyField(field, x * GameSettings.ChunkSize + fx, y * GameSettings.ChunkSize + fy);
                    return field;
                });
                return chunk;
            });

            gen.GenerateCitiesAndFactories();
        }
    }
}
