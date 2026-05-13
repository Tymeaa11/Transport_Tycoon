using VolcanicTransport.Model.Persistence;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.Services
{
    public class SaveLoadService(ISaveFileManager savefileManager, EventHandler<VehicleArrivedEventArgs> vehicleArrivedHandler)
    {
        public GameData LoadGame(string filename)
            => savefileManager.LoadGame(filename, vehicleArrivedHandler);

        public void SaveGame(GameData gameData, string filename)
            => savefileManager.SaveGame(gameData, filename);
    }
}
