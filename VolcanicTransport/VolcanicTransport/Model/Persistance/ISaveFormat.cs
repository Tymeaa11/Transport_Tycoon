namespace VolcanicTransport.Model.Persistance
{
    public interface ISaveFormat : IDisposable
    {
        public void OpenZipForSaving(string filename);
        public void OpenZipForLoading(string filename);

        public void SaveBinaryData(byte[] data);
        public byte[] ReadBinaryData(int totalBytes);

        public void SaveSurfaceSaveData(SurfaceSaveData gameData);
        public SurfaceSaveData LoadSurfaceSaveData();
    }
}
