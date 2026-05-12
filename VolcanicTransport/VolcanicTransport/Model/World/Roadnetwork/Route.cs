using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using VolcanicTransport.Model.Persistance;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class Route : IContainsReference, IHasSavedState
    {
        public string Name { get; set; } = "Névtelen járat";
        [JsonIgnore]
        public ObservableCollection<Station> Stops { get; } = [];

        [JsonInclude]
        public List<string>? SavedStopNames { get; set; }

        public Route() { }

        [JsonConstructor]
        public Route(string name, List<string>? savedStopNames)
        {
            Name = name;
            SavedStopNames = savedStopNames;
        }


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


        public void RestoreReference(Coordinate _ = default)
        {
            if (SavedStopNames == null) return;

            var world = World.Instance;
            Stops.Clear();

            foreach (var stationName in SavedStopNames)
            {
                var station = world.Stations.FirstOrDefault(s => s.StationName == stationName);
                if (station != null)
                    Stops.Add(station);
            }
        }
        public void PrepareForSave()
        {
            SavedStopNames = [.. Stops.Select(s => s.StationName)];
        }
    }
}
