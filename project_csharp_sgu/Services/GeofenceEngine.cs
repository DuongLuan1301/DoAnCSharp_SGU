using project_csharp_sgu.Models;

namespace project_csharp_sgu.Services
{
    public class GeofenceEngine
    {
        POIService _poiService;

        public GeofenceEngine(POIService ps)
        {
            _poiService = ps;
        }

        public POI? Detect(double lat, double lng, double radius = 50)
        {
            if (_poiService.POIs == null || _poiService.POIs.Count == 0)
                return null;

            POI? best = null;
            double min = double.MaxValue;

            foreach (var poi in _poiService.POIs)
            {
                double dist = _poiService.CalculateDistance(lat, lng, poi.Latitude, poi.Longitude);
                if (dist < min && dist <= radius)
                {
                    min = dist;
                    best = poi;
                }
            }

            return best;
        }
    }
}