using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class Route
    {
        private readonly List<Station> _stops;
        private readonly List<Road> _roadToNextStation;
        private readonly bool _isLoop;

        public Route()
        {
            _stops = [];
            _roadToNextStation = [];
            _isLoop = false;
        }

        public Station GetNextStop(Station current)
        {
            // TODO
            return current; //javítandó
        }

        public List<Road> GetRoadsToNextStation()
        {
            // TODO
            return _roadToNextStation; //javítandó
        }
    }
}
