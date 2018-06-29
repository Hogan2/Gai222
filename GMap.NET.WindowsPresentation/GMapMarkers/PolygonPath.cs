using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using static GMap.NET.WindowsPresentation.GMapMarker;

namespace GMap.NET.WindowsPresentation
{
    public class PolygonPath
    {
        GMapControl GMap;
        GMapPolygon gMapRoute;
        List<PointLatLng> PolygonPoints = new List<PointLatLng>();

        public PolygonPath(GMapControl map)
        {
            GMap = map;
        }

        public void Drawing(int id, List<PointLatLng> pos)
        {
            if (pos.Count > 2)
            {
                gMapRoute = new GMapPolygon((int)GMapMarkers_ID.PolygonPath + id,
                (int)GMapMarkers_ZIndex.PolygonPath, pos, Colors.Magenta, 1.0, 0.4, GMap);
            }
        }

        public void Remove(int id)
        {
            GMap.RemoveMarkerAtID(GMap, (int)GMapMarkers_ID.PolygonPath + id);
        }

        public void Update(int id, List<PointLatLng> pos)
        {
            Remove(id);
            Drawing(id, pos);
        }
    }
}
