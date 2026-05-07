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
    public class SaveFileManager : ISaveFileManager
    {
        public ISaveFormat SaveFormat { get; init; }

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        public GameData LoadGame(string filename, EventHandler<VehicleArrivedEventArgs> vehicleArrived)
        {
            using var archive = ZipFile.OpenRead(filename);

            var mapEntry = archive.GetEntry("map.bin");
            var surfaceEntry = archive.GetEntry("surface.json");

            if (mapEntry is null || surfaceEntry is null) throw new LoadingException();

            using var jsonStream = surfaceEntry.Open();
            var surfaceData = JsonSerializer.Deserialize<SurfaceSaveData>(jsonStream);

            World.World.Initialise(surfaceData.SizeInChunks.X, surfaceData.WorldSeed);

            var world = World.World.Instance;

            using (var binStream = mapEntry.Open())
            {
                LoadBinaryMap(binStream);
            }

            world.Cities.AddRange(surfaceData.Cities);
            world.Factories.AddRange(surfaceData.Factories);
            

            RestoreSurfaceElements(surfaceData.Surfaces);
            FinalizeRoadNetwork();

            foreach (var route in surfaceData.Routes)
            {
                route.RestoreReference();
                world.SavedRoutes.Add(route);
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

        public void SaveGame(GameData game, string filename) // save.zip
        {
            using FileStream zipToOpen = new(filename, FileMode.Create);
            using ZipArchive archive = new(zipToOpen, ZipArchiveMode.Create);

            var mapEntry = archive.CreateEntry("map.bin");
            using (var mapStream = mapEntry.Open())
            using (var writer = new BinaryWriter(mapStream))
            {
                SaveBinaryMap(writer);
            }

            foreach (var vehicle in game.World.Vehicles)
                vehicle.PrepareForSave();

            foreach (var r in game.World.SavedRoutes) 
                r.PrepareForSave();

            var surfaceEntry = archive.CreateEntry("surface.json");
            using var jsonStream = surfaceEntry.Open();
            var saveData = new SurfaceSaveData(
                game.World.WorldSeed,
                game.World.SizeInChunks,
                game.Time,
                game.IsPaused,
                game.PlayerMoney,
                game.World.Cities,
                game.World.Factories,
                game.World.Vehicles,
                game.World.SavedRoutes,
                GetSurfaceElements()
            );

            JsonSerializer.Serialize(jsonStream, saveData, _jsonOptions);
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

        private static void SaveBinaryMap(BinaryWriter writer)
        {
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

                    writer.Write(data);
                })
            );
        }

        private static void LoadBinaryMap(Stream binStream)
        {
            var totalBytes = GetTotalBytes();
            var index = 0;
            var bytes = new byte[totalBytes];

            while (index < totalBytes && binStream.CanRead)
            {
                var b = binStream.ReadByte();
                if (b == -1) throw new LoadingException();
                bytes[index++] = (byte)b;
            }

            byte high;
            byte low;

            if (index != totalBytes) throw new LoadingException();

            index = 0;
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
                    throw new LoadingException();

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
