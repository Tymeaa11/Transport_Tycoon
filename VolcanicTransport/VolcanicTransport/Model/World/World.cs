using System.Drawing;
using VolcanicTransport.Model.TerrainGeneration;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World
{
    public class World
    {
        
        #region static fields
        public static readonly Random SharedRandom = new();
        #endregion
        
        #region Fields
        public long WorldSeed { get; private set; }

        public Coordinate SizeInChunks { get; init; }
        public Coordinate SizeInFields { get; init; }
        public RoadNetworkGraph roadnetwork { get; set; }
        public List<City> Cities { get; set; } = new List<City>();
        public List<Factory> Factories { get; set; } = new List<Factory>();
        public List<Station> Stations { get; set; } = new List<Station>();
        private List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        private GameWorldGenerator _gameWorldGenerator;
        private SquareMatrixIterator<Chunk> ChunkMatrix { get; }

        #endregion

        #region Instance
        public class WorldNotInitialisedException : Exception { }

        private static World? _instance;
        
        private World(int worldSize, GameWorldGenerator gameWorldGenerator)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldSize);

            SizeInChunks = new Coordinate(worldSize);
            SizeInFields = SizeInChunks * Chunk.ChunkSize;
            _gameWorldGenerator = gameWorldGenerator;

            ChunkMatrix = new SquareMatrixIterator<Chunk>(worldSize);
            
            InitialiseWorld();
        }
        
        public static World Instance => _instance ?? throw new WorldNotInitialisedException();

        public static void Initialise(int worldSize, GameWorldGenerator gameWorldGenerator)
        {
            if (_instance != null) throw new InvalidOperationException("World already initialised");
            
            _instance = new World(worldSize, gameWorldGenerator);
        }
        #endregion

        public Field? GetField(Coordinate fieldCoordinate)
        {
            if (!fieldCoordinate.IsInside(SizeInFields))
                return null;

            var chunkCoordinate = fieldCoordinate / Chunk.ChunkSize;
            var fieldInChunkCoordinate = fieldCoordinate % Chunk.ChunkSize;

            return ChunkMatrix[chunkCoordinate].FieldMatrix[fieldInChunkCoordinate];
        }

        public Chunk? GetChunk(Coordinate chunkCoordinate)
        {
            if (!chunkCoordinate.IsInside(SizeInChunks))
                return null;

            return ChunkMatrix[chunkCoordinate];
        }
        
        private void InitialiseWorld()
        {
            ChunkMatrix.SetEach((x, y) => new Chunk(new Coordinate(x, y)));
            ChunkMatrix.ReadEach((_,_,c) => c.FieldMatrix.SetEach((_,_) => new Field()));
        }
        
        public void Generate()
        {
            ChunkMatrix.ReadEach(
                (_,_,c) => c.FieldMatrix.ReadEach(
                    (x,y, f) => f.SetFieldHeight(
                        Math.Abs(x - Chunk.ChunkSize / 2) * Math.Abs(y - Chunk.ChunkSize / 2) * 2
                        )));
        }

        public void AddVehicle(Vehicle v)
        {

            Vehicles.Add(v);

        }

        public void ActivateVehicle(Vehicle v)
        {
            if (!Vehicles.Contains(v)) return;
            //v.Activate();
        }

        public void DeactivateVehicle(Vehicle v)
        {
            if (!Vehicles.Contains(v)) return;
            //v.DeActivate();
        }

        public bool PlaceRoad(Coordinate c)
        {




            return true;
        }
    }
}
