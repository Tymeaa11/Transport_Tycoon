using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork;

public class Bus(string name, ProductType type) : Vehicle(name, GameSettings.BusData, type) { }
public class MiniBus(string name, ProductType type) : Vehicle(name, GameSettings.MiniBusData, type) { }
public class TankerTruck(string name, ProductType type) : Vehicle(name, GameSettings.TankerTruckData, type) { }