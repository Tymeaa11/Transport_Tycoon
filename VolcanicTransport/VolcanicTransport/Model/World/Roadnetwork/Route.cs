using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class Route
    {
        private readonly List<Station> _stops;
        public IReadOnlyList<Station> Stops => _stops;
        private readonly List<Road> _roadToNextStation;
        private readonly bool _isLoop;

        public Route()
        {
            _stops = [];
            _roadToNextStation = [];
            _isLoop = false;
        }
        public void AddStop(Station station)
        {
            if (!_stops.Contains(station)) _stops.Add(station);
        }

        public Station? GetNextStop(Station current)
        {
            if (_stops.Count < 2) return null;
            int index = _stops.IndexOf(current);
            if (index == -1) return _stops[0];

            // Oda-vissza járat vagy körjárat (itt körjáratként kezelem)
            return _stops[(index + 1) % _stops.Count];
        }

        public List<Road> GetRoadsToNextStation()
        {
            // TODO
            return _roadToNextStation; //javítandó
        }
    }
}
