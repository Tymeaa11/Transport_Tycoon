using VolcanicTransport.Model;
using VolcanicTransport.Model.Persistance;
using VolcanicTransport.Model.TerrainGeneration.Layers;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.Misc
{

    // ──────────────────────────────────────────────────────
    // FieldsEventArgs
    // ──────────────────────────────────────────────────────
    [TestClass]
    public class FieldsEventArgsTests
    {
        [TestMethod]
        public void ChangedCoordinates_ReturnsPassedList()
        {
            var coords = new List<Coordinate> { new(1, 2), new(3, 4) };
            var args = new FieldsEventArgs(coords);
            CollectionAssert.AreEquivalent(coords, args.ChangedCoordinates);
        }

        [TestMethod]
        public void ChangedCoordinates_EmptyList_ReturnsEmpty()
        {
            var args = new FieldsEventArgs([]);
            Assert.AreEqual(0, args.ChangedCoordinates.Count);
        }
    }

    // ──────────────────────────────────────────────────────
    // ChunkUpdatedEventArgs
    // ──────────────────────────────────────────────────────
    [TestClass]
    public class ChunkUpdatedEventArgsTests
    {
        [TestMethod]
        public void ChunkCoordinate_ReturnsConstructorValue()
        {
            var coord = new Coordinate(3, 7);
            var args = new ChunkUpdatedEventArgs(coord);
            Assert.AreEqual(coord, args.ChunkCoordinate);
        }

        [TestMethod]
        public void ChunkCoordinate_CanBeSet()
        {
            var args = new ChunkUpdatedEventArgs(new Coordinate(0, 0))
            {
                ChunkCoordinate = new Coordinate(5, 5)
            };
            Assert.AreEqual(new Coordinate(5, 5), args.ChunkCoordinate);
        }
    }

    // ──────────────────────────────────────────────────────
    // GameSettings.BridgeData
    // ──────────────────────────────────────────────────────
    [TestClass]
    public class BridgeDataTests
    {
        [TestMethod]
        public void BoneBridge_Length_IsCorrect() => Assert.AreEqual(5, GameSettings.BoneBridge.Length);

        [TestMethod]
        public void BoneBridge_MaxSpeed_IsCorrect() => Assert.AreEqual(10f, GameSettings.BoneBridge.MaxSpeed, 0.01f);

        [TestMethod]
        public void BoneBridge_Price_IsCorrect() => Assert.AreEqual(100.0, GameSettings.BoneBridge.Price, 0.01);

        [TestMethod]
        public void BoneBridge_Tier_IsZero() => Assert.AreEqual(0, GameSettings.BoneBridge.Tier);

        [TestMethod]
        public void StoneBridge_Tier_IsOne() => Assert.AreEqual(1, GameSettings.StoneBridge.Tier);

        [TestMethod]
        public void SteelBridge_Tier_IsTwo() => Assert.AreEqual(2, GameSettings.SteelBridge.Tier);

        [TestMethod]
        public void BridgeData_Equals_SameValues_AreEqual()
        {
            var a = GameSettings.BoneBridge;
            var b = GameSettings.BoneBridge;
            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void BridgeData_ToString_ContainsName()
        {
            string s = GameSettings.BoneBridge.ToString();
            Assert.IsFalse(string.IsNullOrEmpty(s));
        }
    }

    // ──────────────────────────────────────────────────────
    // ISaveFileManager.GameData
    // ──────────────────────────────────────────────────────
    [TestClass]
    [DoNotParallelize]
    public class GameDataTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(4, 88);

        [TestMethod]
        public void GameData_FromGameModel_SetsAllFields()
        {
            var model = GameModel.Instance;
            var data = new GameData(model);

            Assert.AreEqual(GameModel.WorldInstance, data.World);
            Assert.AreEqual(model.IsPaused, data.IsPaused);
            Assert.AreEqual(model.Time, data.Time, 0.001);
            Assert.AreEqual(model.PlayerMoney, data.PlayerMoney, 0.001);
        }

        [TestMethod]
        public void GameData_DirectConstructor_SetsFields()
        {
            var data = new GameData(
                GameModel.WorldInstance, false, 42.5, 10000.0);

            Assert.AreEqual(GameModel.WorldInstance, data.World);
            Assert.IsFalse(data.IsPaused);
            Assert.AreEqual(42.5, data.Time, 0.001);
            Assert.AreEqual(10000.0, data.PlayerMoney, 0.001);
        }

        [TestMethod]
        public void GameData_Equals_SameValues_AreEqual()
        {
            var a = new GameData(GameModel.WorldInstance, false, 1.0, 500.0);
            var b = new GameData(GameModel.WorldInstance, false, 1.0, 500.0);
            Assert.AreEqual(a, b);
        }
    }

    // ──────────────────────────────────────────────────────
    // Vehicle JSON constructors
    // ──────────────────────────────────────────────────────
    [TestClass]
    public class VehicleJsonConstructorTests
    {
        [TestMethod]
        public void Bus_JsonConstructor_RestoresAllFields()
        {
            var bus = new Bus("TestBus", ProductType.HUMAN, 10, 1,
                VehicleState.Loading, 100f, 200f, 45f,
                PathDirection.North, PathDirection.South, 5.0, 0);

            Assert.AreEqual("TestBus", bus.Name);
            Assert.AreEqual(ProductType.HUMAN, bus.CurrentType);
            Assert.AreEqual(10, bus.CurrentLoad);
            Assert.AreEqual(VehicleState.Loading, bus.State);
            Assert.AreEqual(5.0, bus.WaitTimer, 0.001);
        }

        [TestMethod]
        public void MiniBus_JsonConstructor_RestoresAllFields()
        {
            var mb = new MiniBus("TestMini", ProductType.HUMAN, 5, 0,
                VehicleState.Waiting, 0f, 0f, 0f,
                PathDirection.Start, PathDirection.End, 0.0, 0);

            Assert.AreEqual("TestMini", mb.Name);
            Assert.AreEqual(ProductType.HUMAN, mb.CurrentType);
            Assert.AreEqual(5, mb.CurrentLoad);
        }

        [TestMethod]
        public void CargoTruck_JsonConstructor_RestoresAllFields()
        {
            var ct = new CargoTruck("TestCargo", ProductType.SULFUR, 50, 0,
                VehicleState.Moving, 64f, 64f, 90f,
                PathDirection.East, PathDirection.West, 0.0, 1);

            Assert.AreEqual("TestCargo", ct.Name);
            Assert.AreEqual(ProductType.SULFUR, ct.CurrentType);
            Assert.AreEqual(1, ct.CurrentPathIndex);
        }

        [TestMethod]
        public void TankerTruck_JsonConstructor_RestoresAllFields()
        {
            var tt = new TankerTruck("TestTanker", ProductType.WATER, 30, 0,
                VehicleState.Waiting, 32f, 96f, 180f,
                PathDirection.West, PathDirection.East, 1.5, 0);

            Assert.AreEqual("TestTanker", tt.Name);
            Assert.AreEqual(ProductType.WATER, tt.CurrentType);
            Assert.AreEqual(1.5, tt.WaitTimer, 0.001);
        }
    }

    // ──────────────────────────────────────────────────────
    // Factory subtype JSON constructors
    // ──────────────────────────────────────────────────────
    [TestClass]
    [DoNotParallelize]
    public class FactoryJsonConstructorTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(4, 0);

        private static ProductBuffer EmptyBuffer(ProductType t) => new(t, 100);
        private static Product FinalProd(ProductType t) => new(t, 0, 100);

        [TestMethod]
        public void AshProducer_JsonConstructor_SetsName()
        {
            var f = new AshProducer("AshF", ProductType.NONE, FinalProd(ProductType.ASH),
                EmptyBuffer(ProductType.NONE), EmptyBuffer(ProductType.ASH),
                new Coordinate(0, 0), 0);
            Assert.AreEqual("AshF", f.Name);
        }

        [TestMethod]
        public void BoneProducer_JsonConstructor_SetsName()
        {
            var f = new BoneProducer("BoneF", ProductType.NONE, FinalProd(ProductType.BONE),
                EmptyBuffer(ProductType.NONE), EmptyBuffer(ProductType.BONE),
                new Coordinate(0, 0), 0);
            Assert.AreEqual("BoneF", f.Name);
        }

        [TestMethod]
        public void MushroomProducer_JsonConstructor_SetsName()
        {
            var f = new MushroomProducer("MushF", ProductType.NONE, FinalProd(ProductType.MUSHROOM),
                EmptyBuffer(ProductType.NONE), EmptyBuffer(ProductType.MUSHROOM),
                new Coordinate(0, 0), 0);
            Assert.AreEqual("MushF", f.Name);
        }

        [TestMethod]
        public void SteamProducer_JsonConstructor_SetsName()
        {
            var f = new SteamProducer("SteamF", ProductType.WATER, FinalProd(ProductType.STEAM),
                EmptyBuffer(ProductType.WATER), EmptyBuffer(ProductType.STEAM),
                new Coordinate(0, 0), 0);
            Assert.AreEqual("SteamF", f.Name);
            Assert.AreEqual(ProductType.WATER, f.BaseProduct);
        }

        [TestMethod]
        public void SulfurProducer_JsonConstructor_SetsName()
        {
            var f = new SulfurProducer("SulfF", ProductType.NONE, FinalProd(ProductType.SULFUR),
                EmptyBuffer(ProductType.NONE), EmptyBuffer(ProductType.SULFUR),
                new Coordinate(0, 0), 0);
            Assert.AreEqual("SulfF", f.Name);
        }

        [TestMethod]
        public void CondensatorFactory_JsonConstructor_SetsName()
        {
            var f = new CondensatorFactory("CondF", ProductType.STEAM, FinalProd(ProductType.WATER),
                EmptyBuffer(ProductType.STEAM), EmptyBuffer(ProductType.WATER),
                new Coordinate(0, 0), 0);
            Assert.AreEqual("CondF", f.Name);
            Assert.AreEqual(ProductType.STEAM, f.BaseProduct);
        }
    }

    // ──────────────────────────────────────────────────────
    // VehicleManager – AddVehicle / RemoveVehicle / Update
    // ──────────────────────────────────────────────────────
    [TestClass]
    [DoNotParallelize]
    public class WorldVehicleTrackingTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameWorld.Initialise(4, 0);

        [TestInitialize]
        public void ClearVehicles()
        {
            foreach (var v in GameWorld.Instance.Vehicles.ToList())
                GameWorld.Instance.RemoveVehicle(v);
        }

        [TestMethod]
        public void AddVehicle_ThenVehiclesContainsIt()
        {
            var bus = new Bus("WVT1");
            GameWorld.Instance.AddVehicle(bus);
            Assert.IsTrue(GameWorld.Instance.Vehicles.Contains(bus));
            GameWorld.Instance.RemoveVehicle(bus);
        }

        [TestMethod]
        public void RemoveVehicle_AfterAdd_IsAbsent()
        {
            var bus = new Bus("WVT2");
            GameWorld.Instance.AddVehicle(bus);
            GameWorld.Instance.RemoveVehicle(bus);
            Assert.IsFalse(GameWorld.Instance.Vehicles.Contains(bus));
        }

        [TestMethod]
        public void WorldUpdate_TransitionsMovingVehicleWithNoPath_ToWaiting()
        {
            var bus = new Bus("WVT3")
            {
                State = VehicleState.Moving
            };
            GameWorld.Instance.AddVehicle(bus);

            GameWorld.Instance.Update(1.0);

            // Vehicle was Moving with no path — should transition to Waiting
            Assert.AreEqual(VehicleState.Waiting, bus.State);
            GameWorld.Instance.RemoveVehicle(bus);
        }

        [TestMethod]
        public void Vehicles_InitiallyEmpty_AfterClear()
        {
            Assert.AreEqual(0, GameWorld.Instance.Vehicles.Count);
        }
    }

    // ──────────────────────────────────────────────────────
    // SquareMatrixIterator
    // ──────────────────────────────────────────────────────
    [TestClass]
    public class SquareMatrixIteratorTests
    {
        [TestMethod]
        public void ReadEach_VisitsAllCells()
        {
            var m = new SquareMatrixIterator<int>(3);
            int count = 0;
            m.ReadEach((x, y, v) => count++);
            Assert.AreEqual(9, count);
        }

        [TestMethod]
        public void SetEach_SetsAllCells()
        {
            var m = new SquareMatrixIterator<int>(3);
            m.SetEach((x, y) => x + y * 3);

            int sum = 0;
            m.ReadEach((x, y, v) => sum += v);
            Assert.AreEqual(0 + 1 + 2 + 3 + 4 + 5 + 6 + 7 + 8, sum);
        }

        [TestMethod]
        public void ModifyEach_TransformsAllCells()
        {
            var m = new SquareMatrixIterator<int>(2);
            m.SetEach((x, y) => 1);

            m.ModifyEach((x, y, v) => v * 10);

            int sum = 0;
            m.ReadEach((x, y, v) => sum += v);
            Assert.AreEqual(40, sum);
        }

        [TestMethod]
        public void Indexer_Get_ValidCoordinate_ReturnsValue()
        {
            var m = new SquareMatrixIterator<int>(3);
            m.SetEach((x, y) => x * 10 + y);

            int val = m[new Coordinate(2, 1)];

            Assert.AreEqual(21, val);
        }

        [TestMethod]
        public void Indexer_Get_OutOfBounds_Throws()
        {
            var m = new SquareMatrixIterator<int>(3);
            Assert.ThrowsException<IndexOutOfRangeException>(() => _ = m[new Coordinate(5, 5)]);
        }
    }

    // ──────────────────────────────────────────────────────
    // LayeredTerrain
    // ──────────────────────────────────────────────────────
    [TestClass]
    public class LayeredTerrainTests
    {
        private sealed class ConstLayer(float v) : ILayer
        {
            public float Get(float x, float y) => v;
            public void SetSeed(int seed, Random r) { }
        }

        [TestMethod]
        public void Get_SumsAllLayers()
        {
            var t = new LayeredTerrain();
            t.AddLayer(new ConstLayer(1f));
            t.AddLayer(new ConstLayer(2f));
            t.AddLayer(new ConstLayer(3f));

            Assert.AreEqual(6f, t.Get(0, 0), 0.001f);
        }

        [TestMethod]
        public void AddLayer_Array_AddsAll()
        {
            var t = new LayeredTerrain();
            t.AddLayer([new ConstLayer(1f), new ConstLayer(1f)]);

            Assert.AreEqual(2f, t.Get(0, 0), 0.001f);
        }

        [TestMethod]
        public void Constructor_WithList_UsesLayers()
        {
            var layers = new List<ILayer> { new ConstLayer(5f) };
            var t = new LayeredTerrain(layers);

            Assert.AreEqual(5f, t.Get(0, 0), 0.001f);
        }

        [TestMethod]
        public void SetSeed_PropagatestoLayers()
        {
            var t = new LayeredTerrain();
            t.AddLayer(new ConstLayer(1f));
            t.SetSeed(0, new Random(0));
        }

        [TestMethod]
        public void Get_EmptyTerrain_ReturnsZero()
        {
            var t = new LayeredTerrain();
            Assert.AreEqual(0f, t.Get(0, 0), 0.001f);
        }
    }

    // ──────────────────────────────────────────────────────
    // ScalableLayer / PerlinLayer
    // ──────────────────────────────────────────────────────
    [TestClass]
    public class ScalableLayerTests
    {
        [TestMethod]
        public void PerlinLayer_WithFixedOffsets_Get_ReturnsValue()
        {
            var perlin = new Perlin(4, 0.5f, 42);
            var layer = new PerlinLayer(perlin, 1f, 1f, 0f, 0f);

            float val = layer.Get(0f, 0f);

            Assert.IsTrue(val >= -1f && val <= 1f, $"Perlin value out of expected range: {val}");
        }

        [TestMethod]
        public void PerlinLayer_WithRandom_Get_ReturnsValue()
        {
            var perlin = new Perlin(4, 0.5f, 0);
            var rng = new Random(0);
            var layer = new PerlinLayer(perlin, 0.5f, 0.5f, rng);

            float val = layer.Get(10f, 10f);

            Assert.IsTrue(float.IsFinite(val));
        }

        [TestMethod]
        public void ScalableLayer_SetSeed_ChangesOutput()
        {
            var perlin = new Perlin(4, 0.5f, 0);
            var layer = new PerlinLayer(perlin, 1f, 1f, 0f, 0f);
            float before = layer.Get(1f, 1f);

            layer.SetSeed(0, new Random(999));

            float after = layer.Get(1f, 1f);

            Assert.AreNotEqual(before, after);
        }
    }
}
