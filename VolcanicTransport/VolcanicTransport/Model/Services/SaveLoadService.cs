using VolcanicTransport.Model.Persistance;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.Services
{
    public class SaveLoadService
    {
        private readonly ISaveFileManager _savefileManager;
        private readonly EventHandler<VehicleArrivedEventArgs> _vehicleArrivedHandler;

        public SaveLoadService(ISaveFileManager savefileManager, EventHandler<VehicleArrivedEventArgs> vehicleArrivedHandler)
        {
            _savefileManager = savefileManager;
            _vehicleArrivedHandler = vehicleArrivedHandler;
        }

        public GameData LoadGame(string filename)
            => _savefileManager.LoadGame(filename, _vehicleArrivedHandler);

        public void SaveGame(GameData gameData, string filename)
            => _savefileManager.SaveGame(gameData, filename);
    }
}
