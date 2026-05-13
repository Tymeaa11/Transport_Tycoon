using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;

namespace VolcanicTransport.Model.Services
{
    public class TimerService : IDisposable
    {
        private readonly Func<World.World> _getWorld;
        private readonly EconomyService _economy;
        private readonly ScalableTimer _mushroomGrowthTimer;
        private double _monthlyExpenseAccumulator;

        private World.World WorldInstance => _getWorld();

        public double Time { get; private set; }
        public bool IsPaused { get; private set; }

        public event EventHandler? GamePaused;
        public event EventHandler? GameUnpaused;
        public event EventHandler? TimescaleChanged;
        public event EventHandler? GameAdvanced;

        public TimerService(Func<World.World> getWorld, EconomyService economy)
        {
            _getWorld = getWorld;
            _economy = economy;

            _mushroomGrowthTimer = new ScalableTimer { TimeScale = 1 };
            _mushroomGrowthTimer.Elapsed += (_, _) =>
            {
                if (World.World.IsInitialised())
                    UpdateAllMushrooms(0.1);
            };
        }

        public void StartTimer() => _mushroomGrowthTimer.Start();

        public void SetIsPaused(bool isPaused) => IsPaused = isPaused;
        public void SetTime(double time) => Time = time;

        public void Pause()
        {
            IsPaused = true;
            _mushroomGrowthTimer.TimeScale = 0;
            GamePaused?.Invoke(this, EventArgs.Empty);
        }
        public void UnPause()
        {
            IsPaused = false;
            GameUnpaused?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeTimeSpeed1X()
        {
            UnPause();
            _mushroomGrowthTimer.TimeScale = 1;
            TimescaleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeTimeSpeed2X()
        {
            UnPause();
            _mushroomGrowthTimer.TimeScale = 2;
            TimescaleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeTimeSpeed4X()
        {
            UnPause();
            _mushroomGrowthTimer.TimeScale = 4;
            TimescaleChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Update(double deltaTime)
        {
            if (IsPaused) return;
            _monthlyExpenseAccumulator += deltaTime;

            if (_monthlyExpenseAccumulator >= GameSettings.MonthlyExpenseCycleSeconds)
            {
                bool canAfford = _economy.HandleMonthlyExpenses(WorldInstance.Vehicles.Count);
                if (!canAfford) Pause();
                _monthlyExpenseAccumulator = 0;
            }

            WorldInstance.Update(deltaTime);

            foreach (var factory in WorldInstance.Factories)
                factory.Update(deltaTime, (float)Time);

            foreach (var station in WorldInstance.Stations)
                station.GetWaitingPassengers(deltaTime);

            GameAdvanced?.Invoke(this, EventArgs.Empty);
            Time += deltaTime;
        }

        private void UpdateAllMushrooms(double deltaTime)
        {
            int samplesCount = (int)(GameSettings.SamplesCount * deltaTime);
            HashSet<Chunk> chunksToRedraw = [];
            Random rand = WorldInstance.SharedRandom;

            for (int i = 0; i < samplesCount; i++)
            {
                int x = WorldInstance.SharedRandom.Next(0, WorldInstance.SizeInFields.X);
                int y = WorldInstance.SharedRandom.Next(0, WorldInstance.SizeInFields.Y);
                Coordinate randomCoord = new(x, y);

                Field? field = WorldInstance.GetField(randomCoord);
                if (field == null) continue;
                if (field.Type is < FieldType.LOW_LANDS or > FieldType.HIGH_LANDS) continue;

                if (field.Surface is null && rand.NextDouble() < GameSettings.NewSpreadChance)
                {
                    field.Surface = new Mushroom(randomCoord);
                }
                else if (field.Surface is Mushroom mushroom)
                {
                    (Coordinate? target, bool spread) = mushroom.UpdateMushroom(randomCoord);
                    if (spread && target != null)
                    {
                        var chunk = WorldInstance.GetChunk(WorldInstance.GetChunkCoordinate((Coordinate)target));
                        if (chunk != null) chunksToRedraw.Add(chunk);
                    }
                }
            }

            foreach (var chunk in chunksToRedraw)
                WorldInstance.UpdateChunk(chunk.Coordinate);
        }

        public void Dispose()
        {
            _mushroomGrowthTimer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
