using VolcanicTransport.Model;
using VolcanicTransport.Model.World.Roadnetwork;
using GameWorld = VolcanicTransport.Model.World.World;

namespace VolcanicTransport_Tests.GameModelTests
{
    [TestClass]
    [DoNotParallelize]
    public class GameModelSellVehicleTests
    {
        private static GameModel Model => GameModel.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(4, 77);

        [TestMethod]
        public void SellVehicle_AddsMoney()
        {
            var bus = new Bus("SellMoney");
            GameWorld.Instance.AddVehicle(bus);
            double before = Model.PlayerMoney;
            Model.SellVehicle(bus);
            Assert.IsTrue(Model.PlayerMoney >= before, "Selling should refund 50% of price.");
        }

        [TestMethod]
        public void SellVehicle_FiresVehicleSoldEvent()
        {
            var bus = new Bus("SellEvent");
            GameWorld.Instance.AddVehicle(bus);
            bool fired = false;
            Model.VehicleSold += (_, _) => fired = true;
            Model.SellVehicle(bus);
            Model.VehicleSold -= (_, _) => { };
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public void SellVehicle_RemovesVehicleFromWorld()
        {
            var bus = new Bus("SellRemove");
            GameWorld.Instance.AddVehicle(bus);
            Model.SellVehicle(bus);
            Assert.IsFalse(GameWorld.Instance.HasVehicle(bus));
        }

        [TestMethod]
        public void SellVehicle_VehicleNotInWorld_DoesNothing()
        {
            var bus = new Bus("NotInWorld");
            double before = Model.PlayerMoney;
            Model.SellVehicle(bus);
            Assert.AreEqual(before, Model.PlayerMoney, 0.001);
        }
    }

    [TestClass]
    [DoNotParallelize]
    public class GameModelTimeScaleTests
    {
        private static GameModel Model => GameModel.Instance;

        [ClassInitialize]
        public static void ClassSetup(TestContext _) => GameModel.InitialiseNewGame(4, 78);

        [TestMethod]
        public void ChangeTimeSpeed1X_FiresTimescaleChanged()
        {
            bool fired = false;
            Model.TimescaleChanged += (_, _) => fired = true;
            Model.ChangeTimeSpeed1X();
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public void ChangeTimeSpeed1X_UnpausesModel()
        {
            Model.Pause();
            Model.ChangeTimeSpeed1X();
            Assert.IsFalse(Model.IsPaused);
        }

        [TestMethod]
        public void ChangeTimeSpeed2X_FiresTimescaleChanged()
        {
            bool fired = false;
            Model.TimescaleChanged += (_, _) => fired = true;
            Model.ChangeTimeSpeed2X();
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public void ChangeTimeSpeed4X_FiresTimescaleChanged()
        {
            bool fired = false;
            Model.TimescaleChanged += (_, _) => fired = true;
            Model.ChangeTimeSpeed4X();
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public void Pause_SetsPausedToTrue()
        {
            Model.UnPause();
            Model.Pause();
            Assert.IsTrue(Model.IsPaused);
        }

        [TestMethod]
        public void Update_WhenPaused_DoesNotFireGameAdvanced()
        {
            Model.Pause();
            bool fired = false;
            Model.GameAdvanced += (_, _) => fired = true;
            Model.Update(1.0);
            Assert.IsFalse(fired);
        }
    }
}
