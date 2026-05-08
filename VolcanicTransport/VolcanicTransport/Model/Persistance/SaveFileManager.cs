using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using VolcanicTransport.Model.Exceptions;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.Persistance
{
    public class SaveFileManager(ISaveFormat saveFormat) : ISaveFileManager
    {
        public ISaveFormat SaveFormat { get; init; } = saveFormat;

        public GameData LoadGame(string filename, EventHandler<VehicleArrivedEventArgs> vehicleArrived)
        {
            SaveFormat.OpenZipForLoading(filename);
            var surfaceData = SaveFormat.LoadSurfaceSaveData();

            World.World.Initialise(surfaceData.SizeInChunks.X, surfaceData.WorldSeed);
            var world = World.World.Instance;

            LoadBinaryMap(SaveFormat.ReadBinaryData(GetTotalBytes()));

            SaveFormat.Dispose();

            world.Cities.AddRange(surfaceData.Cities);
            world.Factories.AddRange(surfaceData.Factories);

            RestoreSurfaceElements(surfaceData.Surfaces);
            FinalizeRoadNetwork();

            foreach (var route in surfaceData.Routes)
            {
                route.RestoreReference();
                world.AddRoute(route);
            }

            foreach (var vehicle in surfaceData.Vehicles)
            {
                world.AddVehicle(vehicle);
                world.VehicleManager.AddVehicle(vehicle);
                vehicle.RestoreReference(default);
                vehicle.ArrivedAtStation += vehicleArrived;
            }

            return new GameData(world, surfaceData.IsPaused, surfaceData.Time, surfaceData.PlayerMoney);
        }

        public void SaveGame(GameData game, string filename)
        {
            SaveFormat.OpenZipForSaving(filename);
            SaveFormat.SaveBinaryData(GetBinaryData());

            foreach (var vehicle in game.World.Vehicles)
                vehicle.PrepareForSave();

            foreach (var r in game.World.SavedRoutes) 
                r.PrepareForSave();

            var saveData = new SurfaceSaveData(
                game.World.WorldSeed,
                game.World.SizeInChunks,
                game.Time,
                game.IsPaused,
                game.PlayerMoney,
                game.World.Cities,
                game.World.Factories,
                game.World.Vehicles.ToList(),
                game.World.SavedRoutes.ToList(),
                GetSurfaceElements()
            );

            SaveFormat.SaveSurfaceSaveData( saveData );

            SaveFormat.Dispose();
        }

        #region FieldType
        private const int FieldsPerChunk = GameSettings.ChunkSize * GameSettings.ChunkSize;

        private static int GetTotalBytes() =>
            World.World.Instance.SizeInChunks.X * World.World.Instance.SizeInChunks.Y * FieldsPerChunk;

        private const byte LowMask = 0x0F;
        private const byte MushroomId = 0b0000_0100;
        private const byte MushroomStageMask = 0b0000_0011;
        private const byte RoadId = 0b0000_1000;
        private const byte RoadDataMask = 0b0000_0001;
        private const byte SurfaceTypeMask = 0b0000_1100;

        private static byte[] GetBinaryData()
        {
            var bytes = new byte[GetTotalBytes()];
            var index = 0;
            byte data;
            byte stage;

            World.World.Instance.ChunkMatrix.ReadEach((_, _, chunk) =>
                chunk.FieldMatrix.ReadEach((_, _, field) =>
                {
                    data = (byte)(((byte)field.Type & LowMask) << 4);

                    switch (field.Surface)
                    {
                        case Mushroom mushroom:
                            stage = (byte)((byte)mushroom.GrowthStage & MushroomStageMask);
                            data = (byte)(data | MushroomId | stage);
                            break;

                        case Station:
                        case Bridge: break;

                        case Road road:
                            data = (byte)(data | RoadId | (road.IsReserved ? 0b1 : 0b0));
                            break;
                    }

                    bytes[index++] = data;
                })
            );

            return bytes;
        }

        private static void LoadBinaryMap(byte[] bytes)
        {
            byte high;
            byte low;

            var index = 0;

            World.World.Instance.ChunkMatrix.ReadEach((x, y, chunk)
                => chunk.FieldMatrix.ReadEach((fx, fy, field) =>
                {
                    var b = bytes[index++];
                    high = (byte)(b >> 4);
                    low = (byte)(b & LowMask);

                    field.SetFieldTypeTo((FieldType)high);

                    if (low == 0) return;

                    var coordinate = new Coordinate(x * GameSettings.ChunkSize + fx, y * GameSettings.ChunkSize + fy);

                    field.Surface = (low & SurfaceTypeMask) switch
                    {
                        MushroomId
                            => new Mushroom(coordinate, (MushroomGrowthStage)(low & MushroomStageMask)),

                        RoadId
                            => new Road(coordinate) { IsReserved = (low & RoadDataMask) == 1 },

                        _ => field.Surface
                    };
                        
                })
            );
        }
        #endregion

        #region SurfaceElements
        private static List<SurfaceEntry> GetSurfaceElements()
        {
            var world = World.World.Instance;
            List<SurfaceEntry> surfaces = [];

            world.ChunkMatrix.ReadEach((x, y, chunk) =>
                chunk.FieldMatrix.ReadEach((fx, fy, field) =>
                {
                    if (field.Surface is null) return;
                    var coordinate = new Coordinate(x * GameSettings.ChunkSize + fx, y * GameSettings.ChunkSize + fy);

                    switch (field.Surface)
                    {
                        case Station:
                        case Bridge:
                            surfaces.Add(new SurfaceEntry(coordinate, field.Surface));
                            break;
                        

                        case Mushroom: // stored in binary data, skip
                        case Road:
                            break;

                        default:
                            surfaces.Add(new SurfaceEntry(coordinate, field.Surface));
                            break;
                    }
                })
            );

            return surfaces;
        }

        private static void RestoreSurfaceElements(List<SurfaceEntry> surfaces)
        {
            var world = World.World.Instance;
            foreach (var (coordinate, surface) in surfaces)
            {
                var field = world.GetField(coordinate);

                if (field is null || field.Surface is not null) 
                    throw new PersistanceException();

                field.Surface = surface;

                if (surface is IContainsReference hasReference)
                    hasReference.RestoreReference(coordinate);

                if (surface is Station station)
                    world.Stations.Add(station);
            }

            foreach (var (_, surface) in surfaces)
                if (surface is KnowsNeighbour knowsNeighbour)
                    knowsNeighbour.UpdateNeighbourReferences();
        }

        private static void FinalizeRoadNetwork()
        {
            var world = World.World.Instance;

            world.ChunkMatrix.ReadEach((_, _, chunk) =>
                chunk.FieldMatrix.ReadEach((_, _, field) =>
                {
                    if (field.Surface is KnowsNeighbour kn)
                        kn.UpdateNeighbourReferences();
                })
            );

            world.ChunkMatrix.ReadEach((_, _, chunk) =>
                chunk.FieldMatrix.ReadEach((_, _, field) =>
                {
                    if (field.Surface is Road road && field.Surface is not Bridge)
                    {
                        road.Update();
                        world.Roadnetwork.RegisterNodeIfNeeded(road.Coordinate);
                    }
                })
            );

            world.Roadnetwork.RebuildEdges();
        }

        #endregion
    }
}
