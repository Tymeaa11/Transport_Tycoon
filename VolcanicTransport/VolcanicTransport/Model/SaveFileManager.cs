using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model
{
    public class SaveFileManager : ISaveFileManager
    {
        private class LoadingException : Exception { }

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        #region DataWrappers
        private record SurfaceEntry(Coordinate C, ISurface S);

        private readonly record struct SurfaceSaveData(
            int WorldSeed,
            Coordinate SizeInChunks,
            double Time,
            bool IsPaused,
            double PlayerMoney,
            List<City> Cities,
            List<Factory> Factories,
            //List<Vehicle> Vehicles, // TODO Waiting for observable fix
            List<SurfaceEntry> Surfaces);
        #endregion

        public ISaveFileManager.GameData LoadGame(string filename)
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

            return new ISaveFileManager.GameData(world, surfaceData.IsPaused, surfaceData.Time, surfaceData.PlayerMoney);
        }

        public void SaveGame(ISaveFileManager.GameData game, string filename) // save.zip
        {
            using FileStream zipToOpen = new(filename, FileMode.Create);
            using ZipArchive archive = new(zipToOpen, ZipArchiveMode.Create);

            var mapEntry = archive.CreateEntry("map.bin");
            using (var mapStream = mapEntry.Open())
            using (var writer = new BinaryWriter(mapStream))
            {
                SaveBinaryMap(writer);
            }

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
                GetSurfaceElements()
            );

            JsonSerializer.Serialize(jsonStream, saveData, _jsonOptions);
        }

        #region FieldType
        private const int FieldsPerChunk = GameSettings.ChunkSize * GameSettings.ChunkSize;

        private static int GetTotalBytes() =>
            World.World.Instance.SizeInChunks.X * World.World.Instance.SizeInChunks.Y * FieldsPerChunk;

        private const byte LowMask = 0x0F;
        private const byte MushroomStageMask = 0b0000_0011;
        private const byte MushroomId = 0b0000_0100;
        private const byte RoadId = 0b0000_1000;
        private const byte RoadDataMask = 0b0000_0001;
        private const byte CityBuildingId = 0b0000_1100;
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

                        case Road road:
                            data = (byte)(data | RoadId | (road.IsReserved ? 0b1 : 0b0));
                            break;

                        case CityBuilding:
                            data = (byte)(data | CityBuildingId);
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

                        //CityBuildingId
                            //=> new CityBuilding(),

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

                    if (field.Surface is not Mushroom && field.Surface is not Road && field.Surface is not CityBuilding)
                        surfaces.Add(new SurfaceEntry(coordinate, field.Surface));
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

                if (field is null || field.Surface is not null) throw new LoadingException();

                field.Surface = surface;

                switch (surface)
                {
                    case KnowsNeighbour knowsNeighbour:
                        knowsNeighbour.UpdateNeighbourReferences();
                        break;
                }

            }
        }

        #endregion
    }
}
