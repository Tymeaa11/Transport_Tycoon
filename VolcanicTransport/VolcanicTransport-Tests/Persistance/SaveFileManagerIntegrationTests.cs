using VolcanicTransport.Model;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.GameModelTests
{
    [TestClass]
    [DoNotParallelize]
    public class SaveFileManagerIntegrationTests
    {
        [TestMethod]
        public void SaveAndLoad_EmptyWorld_PreservesWorldSize()
        {
            GameModel.InitialiseNewGame(4, 0);
            var originalSize = GameModel.WorldInstance.SizeInChunks;
            string tempFile = Path.GetTempFileName() + ".zip";

            try
            {
                GameModel.Instance.SaveGame(tempFile);
                Assert.IsTrue(File.Exists(tempFile), "Save file should exist.");

                GameModel.InitialiseLoadedGame(tempFile);
                var loadedSize = GameModel.WorldInstance.SizeInChunks;

                Assert.AreEqual(originalSize.X, loadedSize.X);
                Assert.AreEqual(originalSize.Y, loadedSize.Y);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void SaveAndLoad_WithMushroom_MushroomPresent()
        {
            GameModel.InitialiseNewGame(4, 1);
            var w = GameWorld.Instance;
            string tempFile = Path.GetTempFileName() + ".zip";

            // Place a mushroom manually on an empty field
            Coordinate? mushroomCoord = null;
            for (int y = 0; y < w.SizeInFields.Y; y++)
                for (int x = 0; x < w.SizeInFields.X; x++)
                {
                    var c = new Coordinate(x, y);
                    var f = w.GetField(c);
                    if (f != null && f.Surface == null && f.Type == FieldType.LOW_LANDS)
                    {
                        f.Surface = new Mushroom(c, MushroomGrowthStage.SPROUT);
                        mushroomCoord = c;
                        break;
                    }
                    if (mushroomCoord != null) break;
                }

            if (mushroomCoord == null) return;

            try
            {
                GameModel.Instance.SaveGame(tempFile);
                GameModel.InitialiseLoadedGame(tempFile);

                var field = GameModel.WorldInstance.GetField(mushroomCoord.Value);
                Assert.IsInstanceOfType(field!.Surface, typeof(Mushroom));
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void SaveAndLoad_WithCity_CityPreserved()
        {
            GameModel.InitialiseNewGame(4, 2);
            var w = GameWorld.Instance;
            string tempFile = Path.GetTempFileName() + ".zip";

            // May or may not have cities depending on world generation
            int citiesBeforeSave = w.Cities.Count;

            try
            {
                GameModel.Instance.SaveGame(tempFile);
                GameModel.InitialiseLoadedGame(tempFile);

                Assert.AreEqual(citiesBeforeSave, GameModel.WorldInstance.Cities.Count);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void SaveAndLoad_WithFactory_FactoryPreserved()
        {
            GameModel.InitialiseNewGame(4, 3);
            int factoriesBeforeSave = GameModel.WorldInstance.Factories.Count;
            string tempFile = Path.GetTempFileName() + ".zip";

            try
            {
                GameModel.Instance.SaveGame(tempFile);
                GameModel.InitialiseLoadedGame(tempFile);

                Assert.AreEqual(factoriesBeforeSave, GameModel.WorldInstance.Factories.Count);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void SaveAndLoad_PlayerMoney_Preserved()
        {
            GameModel.InitialiseNewGame(4, 4);
            double moneyBefore = GameModel.Instance.PlayerMoney;
            string tempFile = Path.GetTempFileName() + ".zip";

            try
            {
                GameModel.Instance.SaveGame(tempFile);
                GameModel.InitialiseLoadedGame(tempFile);

                Assert.AreEqual(moneyBefore, GameModel.Instance.PlayerMoney, 0.001);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void SaveAndLoad_WithRoute_RoutePreserved()
        {
            GameModel.InitialiseNewGame(4, 5);
            var route = new Route { Name = "TestRoute" };
            GameModel.WorldInstance.AddRoute(route);
            route.PrepareForSave();
            string tempFile = Path.GetTempFileName() + ".zip";

            try
            {
                GameModel.Instance.SaveGame(tempFile);
                GameModel.InitialiseLoadedGame(tempFile);

                bool found = GameModel.WorldInstance.SavedRoutes.Any(r => r.Name == "TestRoute");
                Assert.IsTrue(found);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void SaveAndLoad_WithCityStation_StationPreserved()
        {
            GameModel.InitialiseNewGame(4, 6);
            var w = GameWorld.Instance;
            string tempFile = Path.GetTempFileName() + ".zip";

            // Add a city and station manually
            var city = new City("SaveLoadCity", new Coordinate(2, 2));
            w.Cities.Add(city);
            var station = new CityStation(city, new Coordinate(2, 2), "SLStop");
            var field = w.GetField(new Coordinate(2, 2));
            if (field == null) return;
            var originalSurface = field.Surface;
            field.Surface = station;
            w.Stations.Add(station);

            try
            {
                GameModel.Instance.SaveGame(tempFile);
                GameModel.InitialiseLoadedGame(tempFile);

                var loadedField = GameModel.WorldInstance.GetField(new Coordinate(2, 2));
                Assert.IsInstanceOfType(loadedField!.Surface, typeof(CityStation));
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
                // restore is not needed since we reinit on load
            }
        }
    }
}
