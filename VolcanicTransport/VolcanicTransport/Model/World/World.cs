using System.Collections.ObjectModel;
using VolcanicTransport.Model.TerrainGeneration;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World
{
    public class World
    {
        public class NoWorldGeneratorProvidedException : Exception {}


        #region Fields

        private int _worldSeed;
        public int WorldSeed { 
            get => _worldSeed;
            private set
            {
                _worldSeed = value;
                SharedRandom = new Random(WorldSeed);
            }
            
        }
        public Random SharedRandom { get; private set; } = new Random();

        public Coordinate SizeInChunks { get; init; }
        public Coordinate SizeInFields { get; init; }
        public RoadNetworkGraph Roadnetwork { get; set; }
        public List<City> Cities { get; set; } = [];
        public List<Factory> Factories { get; set; } = [];
        public List<Station> Stations { get; set; } = [];
        public ObservableCollection<Vehicle> Vehicles { get; } = new ObservableCollection<Vehicle>();

        public GameWorldGenerator? GameWorldGenerator { get; set; }
        public SquareMatrixIterator<Chunk> ChunkMatrix { get; }

        public Vehicle? GetLatestVehicle() => Vehicles.LastOrDefault();

        #endregion

        #region Instance
        public class WorldNotInitialisedException : Exception { }

        private static World? _instance;

        private World(int worldSize, int seed)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldSize);

            SizeInChunks = new Coordinate(worldSize);
            SizeInFields = SizeInChunks * Chunk.ChunkSize;
            ChunkMatrix = new SquareMatrixIterator<Chunk>(worldSize);

            WorldSeed = seed;

            InitialiseWorld();

            Roadnetwork = new RoadNetworkGraph();
        }

        public static World Instance => _instance ?? throw new WorldNotInitialisedException();

        public static void Initialise(int worldSize, int seed)
        {
            if (_instance != null) throw new InvalidOperationException("World already initialised");

            _instance = new World(worldSize, seed);
        }
        #endregion
        
        public Coordinate GetChunkCoordinate(Coordinate fieldCoordinate)
            => fieldCoordinate / Chunk.ChunkSize;

        public Coordinate GetFieldCoordinateInChunk(Coordinate fieldCoordinate)
            => fieldCoordinate % Chunk.ChunkSize;

        private Field GetFieldNoChecks(Coordinate fieldCoordinate)
            => ChunkMatrix[GetChunkCoordinate(fieldCoordinate)]
                .FieldMatrix[GetFieldCoordinateInChunk(fieldCoordinate)];

        public Field? GetField(Coordinate fieldCoordinate)
            => fieldCoordinate.IsInside(SizeInFields) 
            ? GetFieldNoChecks(fieldCoordinate) 
            : null;

        public List<Field> GetArea(Coordinate topLeft, Coordinate topRight)
            => topRight.IsInside(SizeInFields) 
            ? [.. Coordinate.GetArea(topLeft, topRight).Where(c => c.IsInside(SizeInFields)).Select(GetFieldNoChecks)] 
            : [];

        public Chunk? GetChunk(Coordinate chunkCoordinate)
            => chunkCoordinate.IsInside(SizeInChunks)
            ? ChunkMatrix[chunkCoordinate]
            : null;

        private void InitialiseWorld()
        {
            ChunkMatrix.SetEach((x, y) => new Chunk(new(x, y)));
            ChunkMatrix.ReadEach((_, _, c) => c.FieldMatrix.SetEach((_, _) => new Field()));
        }

        public void Generate()
        {
            if (GameWorldGenerator == null) throw new NoWorldGeneratorProvidedException();
            ChunkMatrix.ReadEach(
                (cx, cy, c) => c.FieldMatrix.ReadEach(
                    (x, y, f) => GameWorldGenerator.GenerateField(f, cx * Chunk.ChunkSize + x, cy * Chunk.ChunkSize + y)));

            GameWorldGenerator.GenerateCitiesAndFactories();
        }

        public void Generate(int seed)
        {
            WorldSeed = seed;
            GameWorldGenerator?.SetSeed(seed, SharedRandom);
            Generate();
        }



        public void AddVehicle(Vehicle v) => Vehicles.Add(v);
        public void RemoveVehicle(Vehicle v) => Vehicles.Remove(v);
        public bool HasVehicle(Vehicle v) => Vehicles.Contains(v);

        #region Road Placement Logic

        public void UpdateRoadNetworkAround(Coordinate c)
        {
            List<Coordinate> targets = new()
            {
                c,
                c + Direction.North,
                c + Direction.South,
                c + Direction.East,
                c + Direction.West
            };

            HashSet<Chunk> chunksToRender = new HashSet<Chunk>();

            foreach (var coord in targets)
            {
                var chunkCoord = GetChunkCoordinate(coord);
                var chunk = GetChunk(chunkCoord);
                if (chunk != null)
                {
                    chunksToRender.Add(chunk);
                }
            }

            foreach (var chunk in chunksToRender)
            {
                chunk.TriggerRerender();
            }
        }

        #endregion


        public void Update(double gameDt)
        {
            foreach (var vehicle in Vehicles.ToList())
            {
                vehicle.Update(gameDt);
            }

            // Itt jöhetnének késõbb az épületek frissítései (termelés, stb.)
        }
    }
}
