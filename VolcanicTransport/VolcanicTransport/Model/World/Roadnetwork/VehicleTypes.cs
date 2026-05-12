using System.Numerics;
using System.Text.Json.Serialization;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork;

public class Bus : Vehicle
{
    public Bus(string name) : base(name, GameSettings.BusData) { }

    [JsonConstructor]
    public Bus(string name, ProductType currentType, int currentLoad, int currentStopIndex,
               VehicleState state, float posX, float posY, float angle, PathDirection currentEntry,
               PathDirection currentExit, double waitTimer, int currentPathIndex)
        : base(name, GameSettings.BusData, currentType, currentLoad, currentStopIndex,
               state, posX, posY, angle, currentEntry, currentExit, waitTimer, currentPathIndex)
    { }
}

public class MiniBus : Vehicle
{
    public MiniBus(string name) : base(name, GameSettings.MiniBusData) { }

    [JsonConstructor]
    public MiniBus(string name, ProductType currentType, int currentLoad, int currentStopIndex,
               VehicleState state, float posX, float posY, float angle, PathDirection currentEntry,
               PathDirection currentExit, double waitTimer, int currentPathIndex)
        : base(name, GameSettings.MiniBusData, currentType, currentLoad, currentStopIndex,
               state, posX, posY, angle, currentEntry, currentExit, waitTimer, currentPathIndex)
    { }
}
public class MiniTankerTruck : Vehicle
{
    public MiniTankerTruck(string name) : base(name, GameSettings.TankerTruckData) { }

    [JsonConstructor]
    public MiniTankerTruck(string name, ProductType currentType, int currentLoad, int currentStopIndex,
               VehicleState state, float posX, float posY, float angle, PathDirection currentEntry,
               PathDirection currentExit, double waitTimer, int currentPathIndex)
        : base(name, GameSettings.MiniTankerTruckData, currentType, currentLoad, currentStopIndex,
               state, posX, posY, angle, currentEntry, currentExit, waitTimer, currentPathIndex)
    { }
}

public class TankerTruck : Vehicle
{
    public TankerTruck(string name) : base(name, GameSettings.TankerTruckData) { }

    [JsonConstructor]
    public TankerTruck(string name, ProductType currentType, int currentLoad, int currentStopIndex,
               VehicleState state, float posX, float posY, float angle, PathDirection currentEntry,
               PathDirection currentExit, double waitTimer, int currentPathIndex)
        : base(name, GameSettings.TankerTruckData, currentType, currentLoad, currentStopIndex,
               state, posX, posY, angle, currentEntry, currentExit, waitTimer, currentPathIndex)
    { }
}

public class CargoTruck : Vehicle
{
    public CargoTruck(string name) : base(name, GameSettings.CargoTruckData) { }

    [JsonConstructor]
    public CargoTruck(string name, ProductType currentType, int currentLoad, int currentStopIndex,
               VehicleState state, float posX, float posY, float angle, PathDirection currentEntry,
               PathDirection currentExit, double waitTimer, int currentPathIndex)
        : base(name, GameSettings.CargoTruckData, currentType, currentLoad, currentStopIndex,
               state, posX, posY, angle, currentEntry, currentExit, waitTimer, currentPathIndex)
    { }
}

public class MiniCargoTruck : Vehicle
{
    public MiniCargoTruck(string name) : base(name, GameSettings.CargoTruckData) { }

    [JsonConstructor]
    public MiniCargoTruck(string name, ProductType currentType, int currentLoad, int currentStopIndex,
               VehicleState state, float posX, float posY, float angle, PathDirection currentEntry,
               PathDirection currentExit, double waitTimer, int currentPathIndex)
        : base(name, GameSettings.MiniCargoTruckData, currentType, currentLoad, currentStopIndex,
               state, posX, posY, angle, currentEntry, currentExit, waitTimer, currentPathIndex)
    { }
}