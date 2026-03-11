using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model
{
    public class GameModel
    {
        private double playerMoney;
        private bool isPaused;
        private DateTime currentTime;
        private ISaveFileManager savefileManager;

        public World.World world { get; set; }
        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        public event EventHandler? moneyChanged;
        public event EventHandler? gameOver;
        public event EventHandler? newGame;
        public event EventHandler? stationBought;
        public event EventHandler? roadBought;
        public event EventHandler? vehicleBought;
        public event EventHandler? gameAdvanced;
        public event EventHandler? gamePaused;
        public event EventHandler? gameUnpaused;
        public event EventHandler? timescaleChanged;
        public event EventHandler? fieldChanged;
        public event EventHandler? vehicleSelectedIndex;

        public void Pause()
        {
            isPaused = true;
            gamePaused?.Invoke(this, EventArgs.Empty);
        }

        public void UnPause()
        {
            isPaused = false;
            gameUnpaused?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeTimeSpeed1X() { /* Időkezelő logika */ timescaleChanged?.Invoke(this, EventArgs.Empty); }
        public void ChangeTimeSpeed2X() { /* Időkezelő logika */ timescaleChanged?.Invoke(this, EventArgs.Empty); }
        public void ChangeTimeSpeed4X() { /* Időkezelő logika */ timescaleChanged?.Invoke(this, EventArgs.Empty); }

        public void Update(float deltaTime)
        {
            if (isPaused) return;
            // Itt frissül a játékidő és a járművek mozgása
        }

        public bool BuyVehicle(Vehicle v)
        {
            if (TryPurchase(v.Price()))
            {
                Vehicles.Add(v);
                vehicleBought?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public void SellVehicle(Vehicle v)
        {
            if (Vehicles.Contains(v))
            {
                AddMoney(v.Price() * 0.5);
                Vehicles.Remove(v);
            }
        }

        public bool TryPurchase(int amount)
        {
            if (playerMoney >= amount)
            {
                playerMoney -= amount;
                moneyChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public void AddMoney(double amount)
        {
            playerMoney += amount;
            moneyChanged?.Invoke(this, EventArgs.Empty);
        }

        //public void SaveGame() => savefileManager?.Save(this);
        //public void LoadGame() => savefileManager?.Load(this);

        private void HandleMonthlyExpenses()
        {
            // Levonja a fenntartási költségeket
            CheckBankruptcy();
        }

        private void CheckBankruptcy()
        {
            if (playerMoney < 0)
            {
                gameOver?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
