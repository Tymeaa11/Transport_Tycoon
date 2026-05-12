using System.Diagnostics;

namespace VolcanicTransport.Model.Services
{
    public class EconomyService
    {
        public double PlayerMoney { get; private set; }

        public event EventHandler? MoneyChanged;
        public event EventHandler? GameOver;

        public EconomyService(double startingMoney)
        {
            PlayerMoney = startingMoney;
        }

        public bool TryPurchase(double amount)
        {
            if (!(PlayerMoney >= amount)) return false;
            PlayerMoney -= amount;
            MoneyChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public void AddMoney(double amount)
        {
            PlayerMoney += amount;
            MoneyChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetPlayerMoney(double money)
        {
            PlayerMoney = money;
        }

        public bool HandleMonthlyExpenses(int vehicleCount)
        {
            double totalExpense = vehicleCount * GameSettings.MonthlyVehicleMaintenanceCost;
            if (totalExpense <= 0) return true;

            bool able = TryPurchase(totalExpense);
            if (!able)
                GameOver?.Invoke(this, EventArgs.Empty);

            Debug.WriteLine($"Havi kiadások levonva: -{totalExpense}$ (Járművek száma: {vehicleCount})");
            return able;
        }
    }
}
