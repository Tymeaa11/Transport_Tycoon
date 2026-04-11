using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork;

public class Bus(string name) : Vehicle(name, GameSettings.BusData) { }
public class MiniBus(string name) : Vehicle(name, GameSettings.MiniBusData) { }
public class TankerTruck(string name) : Vehicle(name, GameSettings.TankerTruckData) { }
public class CargoTruck(string name) : Vehicle(name, GameSettings.CargoTruckData) { }