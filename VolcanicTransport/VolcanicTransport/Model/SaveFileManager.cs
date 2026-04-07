using System.IO.Compression;
using System.Text.Json;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model
{
    public class SaveFileManager : ISaveFileManager
    {
        private class LoadingException : Exception {}

        #region DataWrappers
        private record SurfaceEntry(Coordinate Position, ISurface Surface);

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
            
            using (var binStream = mapEntry.Open()) {
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
            using (var jsonStream = surfaceEntry.Open())
            {
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
                
                JsonSerializer.Serialize(jsonStream, saveData, new JsonSerializerOptions { WriteIndented = true });
            }
        }

        #region FieldType
        private const int FieldsPerChunk = GameSettings.ChunkSize * GameSettings.ChunkSize;

        private static int GetTotalBytes() =>
            World.World.Instance.SizeInChunks.X * World.World.Instance.SizeInChunks.Y * FieldsPerChunk;
        
        private static void SaveBinaryMap(BinaryWriter writer)
        {
            var totalBytes = GetTotalBytes();
            
            var bytes = new byte[totalBytes];
            var index = 0;
            
            World.World.Instance.ChunkMatrix.ReadEach((_, _, chunk) => 
                chunk.FieldMatrix.ReadEach((_, _, field) =>
                    bytes[index++] = (byte)field.Type)
            );

            for (var i = 0; i < totalBytes; i += 2)
                writer.Write((byte)((bytes[i] << 4) | (bytes[i + 1] & 0x0F)));
        }
        
        private static void LoadBinaryMap(Stream binStream)
        {
            var totalBytes = GetTotalBytes();
            
            var bytes = new byte[totalBytes];
            
            var index = 0;
            while (index < totalBytes && binStream.CanRead)
            {
                var b =  binStream.ReadByte();

                if (b == -1) break;

                bytes[index++] = (byte)(b >> 4);
                bytes[index++] = (byte)(b & 0x0F);
            }
            
            if (index != totalBytes) throw new LoadingException();

            index = 0;
            World.World.Instance.ChunkMatrix.ReadEach((_, _, chunk) 
                => chunk.FieldMatrix.ReadEach((_, _, field) 
                    => field.SetFieldTypeTo( (FieldType) bytes[index++])
                )
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
                if (surface is KnowsNeighbour knowsNeighbour)
                {
                    knowsNeighbour.UpdateNeighbourReferences();
                }
                
            }
        }
        
        #endregion
    }
}
