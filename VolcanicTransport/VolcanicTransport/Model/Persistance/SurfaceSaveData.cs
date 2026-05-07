using System.Collections.ObjectModel;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.Persistance
{
    public readonly record struct SurfaceSaveData(
        int WorldSeed,
        Coordinate SizeInChunks,
        double Time,
        bool IsPaused,
        double PlayerMoney,
        List<City> Cities,
        List<Factory> Factories,
        ObservableCollection<Vehicle> Vehicles,
        ObservableCollection<Route> Routes,
        List<SurfaceEntry> Surfaces
    );
}
