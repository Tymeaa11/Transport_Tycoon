using System.Collections.ObjectModel;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class Route
    {
        public string Name { get; set; } = "Névtelen járat";
        public ObservableCollection<Station> Stops { get; } = [];

        //private readonly bool _isLoop;

        public void AddStop(Station station)
        {
            if (!Stops.Contains(station)) Stops.Add(station);
        }

        public Station? GetNextStop(Station current)
        {
            if (Stops.Count < 2) return null;
            int index = Stops.IndexOf(current);
            if (index == -1) return Stops[0];

            return Stops[(index + 1) % Stops.Count];
        }
        public override string ToString() => Name;

    }
}
