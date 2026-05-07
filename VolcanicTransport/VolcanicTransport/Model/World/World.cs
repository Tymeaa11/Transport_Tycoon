using System.Collections.ObjectModel;
using System.Diagnostics;
using VolcanicTransport.Model.Exceptions;
using VolcanicTransport.Model.TerrainGeneration;
using VolcanicTransport.Model.TerrainGeneration.Generators;
using VolcanicTransport.Model.TerrainGeneration.Layers;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World
{
    public class World
    {
        #region Fields

        private int _worldSeed;
        public int WorldSeed
        {
            get => _worldSeed;
            private set
            {
                _worldSeed = value;
                SharedRandom = new Random(WorldSeed);
            }
        }
        public Random SharedRandom { get; private set; } = new();
        public Perlin SharedPerlin { get; private set; }

        public Coordinate SizeInChunks { get; }
        public Coordinate SizeInFields { get; }
        public RoadNetworkGraph Roadnetwork { get; }
        public List<City> Cities { get; } = [];
        public List<Factory> Factories { get; } = [];
        public List<Station> Stations { get; } = [];
        public ObservableCollection<Route> SavedRoutes { get; } = [];
        public ObservableCollection<Vehicle> Vehicles { get; } = [];

        public IWorldGenerator? GameWorldGenerator { get; set; }
        public SquareMatrixIterator<Chunk> ChunkMatrix { get; private set; }

        public event EventHandler<ChunkUpdatedEventArgs>? ChunkChanged;

        public Vehicle? GetLatestVehicle() => Vehicles.LastOrDefault();

        public Roadnetwork.VehicleManager VehicleManager { get; } = new();

        #endregion

        #region Instance
        private static World? _instance;

        private World(int worldSize, int seed)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldSize);

            SizeInChunks = new Coordinate(worldSize);
            SizeInFields = SizeInChunks * GameSettings.ChunkSize;
            ChunkMatrix = new SquareMatrixIterator<Chunk>(worldSize);
            WorldSeed = seed;

            SharedPerlin = new Perlin(4, 0.5f, seed);

            InitialiseWorld();

            Roadnetwork = new RoadNetworkGraph();
            VehicleManager = new VehicleManager();
        }

        public static World Instance => _instance ?? throw new WorldNotInitialisedException();
        public static bool IsInitialised() => _instance is not null;
        public static void Initialise(int worldSize, int seed)
            => _instance = new World(worldSize, seed);
        #endregion

        #region FieldGetters
        public Coordinate GetChunkCoordinate(Coordinate fieldCoordinate)
            => fieldCoordinate / GameSettings.ChunkSize;

        public Coordinate GetFieldCoordinateInChunk(Coordinate fieldCoordinate)
            => fieldCoordinate % GameSettings.ChunkSize;

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

        #endregion

        #region Methods
        private void InitialiseWorld()
        {
            ChunkMatrix.SetEach((x, y) => new Chunk(new Coordinate(x, y)));
            ChunkMatrix.ReadEach((_, _, c) => c.FieldMatrix.SetEach((_, _) => new Field()));
        }

        public void Generate()
        {
            if (GameWorldGenerator == null) throw new NoWorldGeneratorProvidedException();
            ChunkMatrix.ReadEach(
                (cx, cy, c) => c.FieldMatrix.ReadEach(
                    (x, y, f) => GameWorldGenerator.ModifyField(f, cx * GameSettings.ChunkSize + x, cy * GameSettings.ChunkSize + y)));

            GameWorldGenerator.GenerateCitiesAndFactories();

            Debug.WriteLine($"Generated {Cities.Count} cities and {Factories.Count} factories");
        }

        public void Generate(int seed)
        {
            WorldSeed = seed;
            GameWorldGenerator?.SetSeed(seed, SharedRandom);
            Generate();
        }

        public void UpdateChunk(Coordinate chunkCoordinate)
            => ChunkChanged?.Invoke(this, new(chunkCoordinate));


        public void AddVehicle(Vehicle v) => Vehicles.Add(v);
        public void RemoveVehicle(Vehicle v) => Vehicles.Remove(v);
        public bool HasVehicle(Vehicle v) => Vehicles.Contains(v);

        #region Road Placement Logic

        public void UpdateRoadNetworkAround(Coordinate c)
        {
            List<Coordinate> targets =
            [
                c,
                c + Coordinate.North,
                c + Coordinate.South,
                c + Coordinate.East,
                c + Coordinate.West
            ];

            HashSet<Chunk> chunksToRender = [];

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
                UpdateChunk(chunk.Coordinate);
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

        #endregion
    }
}
