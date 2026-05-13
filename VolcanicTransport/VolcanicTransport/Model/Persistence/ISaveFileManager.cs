using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.Persistence
{
    public interface ISaveFileManager
    {
        public ISaveFormat SaveFormat { get; init; }

        public GameData LoadGame(string filename, EventHandler<VehicleArrivedEventArgs> vehicleArrived);
        public void SaveGame(GameData game, string filename);
    }
}
