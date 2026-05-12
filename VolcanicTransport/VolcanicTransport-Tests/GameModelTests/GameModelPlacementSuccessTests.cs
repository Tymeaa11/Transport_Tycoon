using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;
using static VolcanicTransport.Model.World.Roadnetwork.Vehicle;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.GameModelTests
{
    [TestClass]
    [DoNotParallelize]
    public class GameModelPlaceRoadSuccessTests
    {
        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(4, 200);

        private static GameModel Model => GameModel.Instance;
        private static GameWorld W => GameWorld.Instance;

        private static void ClearAdjacentRoads(Coordinate coord)
        {
            foreach (var dir in Coordinate.Directions)
            {
                var nf = W.GetField(coord + dir);
                if (nf?.Surface is Road) nf.Surface = null;
            }
        }

        [TestMethod]
        public void PlaceRoad_OnIsolatedLowLands_PlacesRoad()
        {
            var coord = new Coordinate(10, 10);
            var field = W.GetField(coord)!;
            var origType = field.Type;
            var origSurface = field.Surface;

            field.SetFieldTypeTo(FieldType.LOW_LANDS);
            field.Surface = null;
            ClearAdjacentRoads(coord);

            Model.PlaceRoad(coord);

            bool roadPlaced = field.Surface is Road;

            field.SetFieldTypeTo(origType);
            field.Surface = origSurface;

            Assert.IsTrue(roadPlaced);
        }

        [TestMethod]
        public void PlaceRoad_OnIsolatedLowLands_DeductsMoney()
        {
            var coord = new Coordinate(11, 10);
            var field = W.GetField(coord)!;
            field.SetFieldTypeTo(FieldType.LOW_LANDS);
            field.Surface = null;
            ClearAdjacentRoads(coord);

            double before = Model.PlayerMoney;
            Model.PlaceRoad(coord);

            field.Surface = null;

            Assert.IsTrue(Model.PlayerMoney < before);
        }

        [TestMethod]
        public void PlaceBridge_ValidHorizontalConfig_ReturnsTrue()
        {
            int x0 = 20, y = 20;
            var bridgeType = GameSettings.BridgeTypes[0];
            int len = bridgeType.Length; // 5

            var origTypes = new FieldType[len];
            var origSurfaces = new ISurface?[len];
            for (int i = 0; i < len; i++)
            {
                var f = W.GetField(new Coordinate(x0 + i, y))!;
                origTypes[i] = f.Type;
                origSurfaces[i] = f.Surface;
                f.Surface = null;
            }

            for (int i = 1; i < len - 1; i++)
            {
                var fn = W.GetField(new Coordinate(x0 + i, y - 1));
                var fs = W.GetField(new Coordinate(x0 + i, y + 1));
                if (fn?.Surface is Road) fn.Surface = null;
                if (fs?.Surface is Road) fs.Surface = null;
            }

            W.GetField(new Coordinate(x0, y))!.SetFieldTypeTo(FieldType.LOW_LANDS);
            W.GetField(new Coordinate(x0 + len - 1, y))!.SetFieldTypeTo(FieldType.LOW_LANDS);
            for (int i = 1; i < len - 1; i++)
                W.GetField(new Coordinate(x0 + i, y))!.SetFieldTypeTo(FieldType.LAVA_OCEAN);

            bool result = Model.PlaceBridge(
                new Coordinate(x0, y),
                new Coordinate(x0 + len - 1, y),
                bridgeType);

            for (int i = 0; i < len; i++)
            {
                var f = W.GetField(new Coordinate(x0 + i, y))!;
                f.SetFieldTypeTo(origTypes[i]);
                f.Surface = origSurfaces[i];
            }

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PlaceBridge_ValidVerticalConfig_ReturnsTrue()
        {
            int x = 25, y0 = 30;
            var bridgeType = GameSettings.BridgeTypes[0];
            int len = bridgeType.Length;

            var origTypes = new FieldType[len];
            var origSurfaces = new ISurface?[len];
            for (int i = 0; i < len; i++)
            {
                var f = W.GetField(new Coordinate(x, y0 + i))!;
                origTypes[i] = f.Type;
                origSurfaces[i] = f.Surface;
                f.Surface = null;
            }

            for (int i = 1; i < len - 1; i++)
            {
                var fw = W.GetField(new Coordinate(x - 1, y0 + i));
                var fe = W.GetField(new Coordinate(x + 1, y0 + i));
                if (fw?.Surface is Road) fw.Surface = null;
                if (fe?.Surface is Road) fe.Surface = null;
            }

            W.GetField(new Coordinate(x, y0))!.SetFieldTypeTo(FieldType.LOW_LANDS);
            W.GetField(new Coordinate(x, y0 + len - 1))!.SetFieldTypeTo(FieldType.LOW_LANDS);
            for (int i = 1; i < len - 1; i++)
                W.GetField(new Coordinate(x, y0 + i))!.SetFieldTypeTo(FieldType.LAVA_OCEAN);

            bool result = Model.PlaceBridge(
                new Coordinate(x, y0),
                new Coordinate(x, y0 + len - 1),
                bridgeType);

            for (int i = 0; i < len; i++)
            {
                var f = W.GetField(new Coordinate(x, y0 + i))!;
                f.SetFieldTypeTo(origTypes[i]);
                f.Surface = origSurfaces[i];
            }

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void HeightenField_OnLowLands_RaisesFieldType()
        {
            var coord = new Coordinate(40, 40);
            var field = W.GetField(coord)!;
            field.SetFieldTypeTo(FieldType.LOW_LANDS);
            field.Surface = null;

            Model.HeightenField(coord);

            Assert.AreNotEqual(FieldType.LOW_LANDS, field.Type);

            field.SetFieldTypeTo(FieldType.LOW_LANDS);
        }

        [TestMethod]
        public void LowerField_OnLowLands_LowersFieldType()
        {
            var coord = new Coordinate(41, 40);
            var field = W.GetField(coord)!;
            field.SetFieldTypeTo(FieldType.LOW_LANDS);
            field.Surface = null;

            Model.LowerField(coord);

            Assert.AreNotEqual(FieldType.LOW_LANDS, field.Type);

            field.SetFieldTypeTo(FieldType.LOW_LANDS);
        }

        [TestMethod]
        public void HeightenField_WithSproutMushroom_DeductsExtraCost()
        {
            var coord = new Coordinate(42, 40);
            var field = W.GetField(coord)!;
            field.SetFieldTypeTo(FieldType.LOW_LANDS);
            field.Surface = new Mushroom(coord, MushroomGrowthStage.SPROUT);

            double before = Model.PlayerMoney;
            Model.HeightenField(coord);
            double spent = before - Model.PlayerMoney;

            field.Surface = null;

            double expected = GameSettings.BaseTerraformationPrice + GameSettings.MushroomPricePerUnit;
            Assert.AreEqual(expected, spent, 0.001);
        }

        [TestMethod]
        public void PlaceRoad_WithSproutMushroom_DeductsExtraCost()
        {
            var coord = new Coordinate(43, 40);
            var field = W.GetField(coord)!;
            field.SetFieldTypeTo(FieldType.LOW_LANDS);
            field.Surface = new Mushroom(coord, MushroomGrowthStage.SPROUT);
            ClearAdjacentRoads(coord);

            double before = Model.PlayerMoney;
            Model.PlaceRoad(coord);
            double spent = before - Model.PlayerMoney;

            field.Surface = null;

            double expected = GameSettings.BaseRoadPrice + GameSettings.MushroomPricePerUnit;
            Assert.AreEqual(expected, spent, 0.001);
        }

        [TestMethod]
        public void PlaceStation_NearCity_WithAdjacentRoad_ReturnsTrue()
        {
            var stationCoord = new Coordinate(50, 50);
            var roadCoord = new Coordinate(51, 50);
            var cityCoord = new Coordinate(52, 50);

            var city = new City("PSCity", cityCoord);
            W.Cities.Add(city);

            var stationField = W.GetField(stationCoord)!;
            var roadField = W.GetField(roadCoord)!;

            var origStationType = stationField.Type;
            var origStationSurface = stationField.Surface;
            var origRoadType = roadField.Type;
            var origRoadSurface = roadField.Surface;

            stationField.SetFieldTypeTo(FieldType.LOW_LANDS);
            stationField.Surface = null;

            roadField.SetFieldTypeTo(FieldType.LOW_LANDS);
            var road = new Road(roadCoord);
            roadField.Surface = road;
            road.Update();

            var blockers = W.Stations.Where(s => s.Coordinate.Distance(stationCoord) <= 3).ToList();
            foreach (var b in blockers) W.Stations.Remove(b);

            bool result = Model.PlaceStation(stationCoord);

            W.Cities.Remove(city);
            if (!result)
            {
                stationField.SetFieldTypeTo(origStationType);
                stationField.Surface = origStationSurface;
            }
            roadField.SetFieldTypeTo(origRoadType);
            roadField.Surface = origRoadSurface;
            foreach (var b in blockers) W.Stations.Add(b);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PlaceStation_NearFactory_WithAdjacentRoad_ReturnsTrue()
        {
            var stationCoord = new Coordinate(60, 60);
            var roadCoord = new Coordinate(61, 60);
            var factoryCoord = new Coordinate(62, 60);

            var factory = new SulfurProducer("PSFactory", factoryCoord);
            W.Factories.Add(factory);

            var stationField = W.GetField(stationCoord)!;
            var roadField = W.GetField(roadCoord)!;

            stationField.SetFieldTypeTo(FieldType.LOW_LANDS);
            stationField.Surface = null;

            roadField.SetFieldTypeTo(FieldType.LOW_LANDS);
            var road = new Road(roadCoord);
            roadField.Surface = road;
            road.Update();

            var blockers = W.Stations.Where(s => s.Coordinate.Distance(stationCoord) <= 3).ToList();
            foreach (var b in blockers) W.Stations.Remove(b);

            bool result = Model.PlaceStation(stationCoord);

            W.Factories.Remove(factory);
            if (!result)
            {
                stationField.Surface = null;
            }
            roadField.Surface = null;
            foreach (var b in blockers) W.Stations.Add(b);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AddStopToVehicle_WhenWaiting_AddsStopAndTriggersRoute()
        {
            var bus = new Bus("AddStop_Bus");
            var route = new Route();
            var s1 = PlainStation.MakePlainStation(new Coordinate(70, 70));
            var s2 = PlainStation.MakePlainStation(new Coordinate(71, 70));
            route.AddStop(s1);
            bus.Route = route;
            bus.State = VehicleState.Waiting;

            bool routeChangedFired = false;
            bus.RouteChanged += (_, _) => routeChangedFired = true;

            GameModel.AddStopToVehicle(bus, s2);

            Assert.AreEqual(2, bus.Route.Stops.Count);
            Assert.IsTrue(routeChangedFired);
        }

        [TestMethod]
        public void AddStopToVehicle_DuplicateLastStop_DoesNotAdd()
        {
            var bus = new Bus("DStop_Bus");
            var route = new Route();
            var s1 = PlainStation.MakePlainStation(new Coordinate(72, 70));
            route.AddStop(s1);
            bus.Route = route;

            GameModel.AddStopToVehicle(bus, s1);

            Assert.AreEqual(1, bus.Route.Stops.Count);
        }

    }

    file class PlainStation(Coordinate coord) : Station(
        coord, "Plain",
        new ProductBuffer(ProductType.HUMAN, 10),
        new Product(ProductType.HUMAN, 0, 10))
    {
        public override int UnLoadProductFromVehicle(Vehicle vehicle) => 0;

        public static PlainStation MakePlainStation(Coordinate coord)
        {
            var f = GameWorld.Instance.GetField(coord);
            var s = new PlainStation(coord);
            if (f != null)
            {
                f.SetFieldTypeTo(FieldType.LOW_LANDS);
                f.Surface = s;
            }
            return s;
        }
    }
}
