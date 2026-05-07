using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using VolcanicTransport.Model.Exceptions;

namespace VolcanicTransport.Model.Persistance
{
    public class Zip2FileSaveFormat : ISaveFormat
    {
        #region Fields
        private ZipArchive? _zipArchive;
        private ZipArchiveEntry? _mapEntry;
        private ZipArchiveEntry? _surfaceEntry;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        #endregion

        public Zip2FileSaveFormat() { }

        public void OpenZipForSaving(string filename) 
        {
            Dispose();
            _zipArchive = new(new FileStream(filename, FileMode.Create), ZipArchiveMode.Create);

            _mapEntry = _zipArchive.CreateEntry("map.bin");
            _surfaceEntry = _zipArchive.CreateEntry("surface.json");

            if (_mapEntry == null || _surfaceEntry == null)
                throw new PersistanceException("Error opening zip archive for saving.");
        }

        public void OpenZipForLoading(string filename) 
        {
            Dispose();
            _zipArchive = ZipFile.OpenRead(filename);
            _mapEntry = _zipArchive.GetEntry("map.bin");
            _surfaceEntry = _zipArchive.GetEntry("surface.json");

            if (_zipArchive == null || _mapEntry == null || _surfaceEntry == null)
                throw new PersistanceException("Error opening zip archive for loading.");
        }

        public void SaveSurfaceSaveData(SurfaceSaveData surfaceSaveData)
        {
            if (_surfaceEntry == null)
                throw new PersistanceException("Unable to save JSON data.");

            using var jsonStream = _surfaceEntry.Open();
            JsonSerializer.Serialize(jsonStream, surfaceSaveData, _jsonOptions);
        }

        public SurfaceSaveData LoadSurfaceSaveData()
        {
            if (_surfaceEntry == null)
                throw new PersistanceException("Unable to load JSON data.");

            using var jsonStream = _surfaceEntry.Open();
            return JsonSerializer.Deserialize<SurfaceSaveData>(jsonStream);
        }

        public byte[] ReadBinaryData(int totalBytes)
        {
            if (_mapEntry == null)
                throw new PersistanceException("Unable to load binary data.");

            var bytes = new byte[totalBytes];
            using var binStream = _mapEntry.Open();

            try
            {
                binStream.ReadExactly(bytes, 0, totalBytes);
            }
            catch (EndOfStreamException)
            {
                throw new PersistanceException("Bytes read doesn't match bytes expected.");
            }

            return bytes;
        }

        public void SaveBinaryData(byte[] data)
        {
            if (_mapEntry == null || data == null)
                throw new PersistanceException("Unable to save binary data.");

            using var mapStream = _mapEntry.Open();
            using var writer = new BinaryWriter(mapStream);

            writer.Write(data);
        }

        public void Dispose()
        {
            _zipArchive?.Dispose();
        }
    }
}
