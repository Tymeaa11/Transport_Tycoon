namespace VolcanicTransport.Model.Persistance
{
    public interface ISaveFormat
    {
        public void SaveBinaryData(BinaryWriter binaryWriter);
        public void ReadBinaryData(Stream binaryData);
        public List<SurfaceEntry> GetSurfaceEntries();
        public void SaveGameData(GameData gameData);
        public GameData LoadGameData();
    }
}
